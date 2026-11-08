import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

vi.mock('../config', () => ({
  getConfig: () => ({
    apiUrl: 'https://api.test/',
    appUrl: '',
    appState: 'Production',
    messageCzech: '',
    messageEnglish: '',
    cities: ['prague'],
  }),
}))

import { useMapStore } from './map'
import { setMapController, type MapController } from '../map/controller'
import type { MapObject } from '../models/api'

const pragueSettings = readFileSync(resolve(__dirname, '../../public/mapsettings.prague.json'), 'utf8').replace(/^﻿/, '')

type FetchCall = { url: string; body: any }
let calls: FetchCall[]
let apiResponses: Record<string, unknown>

function jsonResponse(data: unknown, contentType = 'application/json') {
  return new Response(typeof data === 'string' ? data : JSON.stringify(data), {
    status: 200,
    headers: { 'content-type': contentType },
  })
}

function controllerSpy() {
  const spy = {} as Record<keyof MapController, ReturnType<typeof vi.fn>>
  const names: (keyof MapController)[] = [
    'initMap', 'destroyMap', 'refreshObjectsOnMap', 'addObjects', 'removeAdditionalObjects', 'selectPointOnMap',
    'unselectAllSelectedPoints', 'showLayersForNormalMap', 'hideLayersForNormalMap', 'toggleLayerGroup',
    'resetMapViewToInitialState', 'toggleScaleVisibility', 'fitMapToWindow', 'goToLocation', 'zoomIn', 'zoomOut',
    'getZoom', 'getMapBounds', 'turnOnLocationTracking', 'goToMyLocation', 'setDialogFullScreen', 'requestRefresh',
    'setUrlFromSelectedPlace', 'resizePhotoHeight', 'closeGallery',
  ]
  for (const n of names) spy[n] = vi.fn()
  setMapController(spy as unknown as MapController)
  return spy
}

beforeEach(() => {
  setActivePinia(createPinia())
  calls = []
  apiResponses = {}
  vi.stubGlobal('fetch', vi.fn(async (input: string, init?: RequestInit) => {
    const url = String(input)
    calls.push({ url, body: init?.body ? JSON.parse(String(init.body)) : null })
    if (url.includes('mapsettings.prague.json')) return jsonResponse(pragueSettings)
    if (url.includes('mapsettings.')) return jsonResponse('<!doctype html>', 'text/html')
    const method = url.replace('https://api.test/', '').split('?')[0]
    return jsonResponse(apiResponses[method] ?? { $id: '1', $values: [] })
  }))
})

afterEach(() => {
  vi.unstubAllGlobals()
  setMapController(null)
})

describe('init', () => {
  it('načte nastavení, výchozí výběr vrstev a časové osy', async () => {
    const store = useMapStore()
    expect(await store.init('prague')).toBe(true)
    expect(store.city).toBe('prague')
    expect(store.mapState).toBe('Initialized')
    const layers = store.map!.layers!
    // bez parametru layers: vybrané vše kromě heatmapy
    expect(layers.filter((a) => a.type !== 'Heatmap').every((a) => a.selected)).toBe(true)
    expect(layers.filter((a) => a.type === 'Heatmap').every((a) => !a.selected)).toBe(true)
    // výchozí bod časové osy je ten bez data "from"
    expect(store.timeline!.filter((a) => a.selected).map((a) => a.name)).toEqual(['all'])
    expect(store.timelinePoint).toBeNull()
  })

  it('vrstvy a bod časové osy z URL', async () => {
    const store = useMapStore()
    await store.init('prague', 'address,incident', '1942_Q2')
    const selected = store.map!.layers!.filter((a) => a.selected).map((a) => a.name)
    expect(selected).toEqual(expect.arrayContaining(['OSM', 'address', 'incident']))
    expect(selected).not.toContain('interest')
    expect(store.getLayersForUrlParameter()).toBe('address,incident')
    expect(store.getTimelineForUrlParameter()).toBe('1942_Q2')
    expect(store.timelinePoint).toBe('1942-04-01')
  })

  it('neexistující město vrátí false', async () => {
    const store = useMapStore()
    expect(await store.init('atlantis')).toBe(false)
    expect(store.map).toBeNull()
  })
})

describe('časová osa', () => {
  it('toggleTimelinePoint vybere právě jeden bod a druhým klikem ho odznačí', async () => {
    const store = useMapStore()
    await store.init('prague')
    const q1 = store.timeline!.find((a) => a.name === '1942_Q1')!
    store.toggleTimelinePoint(q1)
    expect(store.timeline!.filter((a) => a.selected).map((a) => a.name)).toEqual(['1942_Q1'])
    store.toggleTimelinePoint(q1)
    expect(store.timeline!.filter((a) => a.selected)).toHaveLength(0)
  })
})

describe('getMapObjects', () => {
  it('posílá parametry jako C# klient a respektuje zoom vrstev', async () => {
    const store = useMapStore()
    await store.init('prague', null, '1942_Q2')
    store.mapZoom = 16
    store.setBBox([{ x: 14.4, y: 50.0 }, { x: 14.5, y: 50.1 }])
    await store.getMapObjects(false, true)

    const call = calls.find((c) => c.url.endsWith('getmapobjects'))!
    expect(call.body).toMatchObject({
      city: 'prague',
      mapSouthWestPoint: { x: 14.4, y: 50.0 },
      mapNorthEastPoint: { x: 14.5, y: 50.1 },
      selectedTimeLinePoint: { name: '1942_Q2', from: '1942-04-01', to: '1942-06-30' },
      customCoordinates: null,
    })
    // PascalCase názvy typů míst, bez polygonů (withPolygons = false), bez heatmapy a statistik bez placeType vrstvy objektů
    expect(call.body.selectedLayerNames).toEqual(expect.arrayContaining(['Address', 'Interest', 'Incident']))
    expect(call.body.selectedLayerNames).not.toContain('Inaccessible')
    // vnořené vrstvy časové osy se do API neposílají
    expect(call.body.selectedTimeLinePoint).not.toHaveProperty('additionalLayers')
  })

  it('agreguje adresy se stejnými souřadnicemi', () => {
    const store = useMapStore()
    const obj = (id: number, labelCs: string, x: number, citizens: number | null): MapObject => ({
      placeType: 'Address', layerName: 'address', citizens, citizensTotal: 10, id, labelCs, labelEn: labelCs + 'E',
      geographyMapPoint: { type: 'Point', coordinates: [x, 50] }, geographyMapPolygon: null,
      mapPoint: null, mapPolygon: null, dateFrom: null, dateTo: null,
    })
    const result = store.aggregateSameAddresses([obj(1, 'B', 14, 2), obj(2, 'A', 14, 3), obj(3, 'C', 15, 1)])
    expect(result).toHaveLength(2)
    const merged = result.find((a) => a.geographyMapPoint && (a.geographyMapPoint.coordinates as number[])[0] === 14)!
    expect(merged).toMatchObject({ id: 2, citizens: 5, citizensTotal: 20, labelCs: 'A <br/> B', labelEn: 'AE <br/> BE' })
  })
})

describe('getDistrictStatistics', () => {
  it('celkové statistiky při malém zoomu, části města při větším, nic při velkém', async () => {
    const store = useMapStore()
    await store.init('prague')
    const layer = store.getNotBaseLayers().find((a) => a.placeType === 'Statistics')!
    expect(layer).toBeTruthy()

    store.mapZoom = (layer.minZoom ?? 0) - 1
    await store.getDistrictStatistics()
    expect(calls.at(-1)!.url).toContain('total=true')

    store.mapZoom = layer.minZoom!
    await store.getDistrictStatistics()
    expect(calls.at(-1)!.url).toContain('total=false')

    const before = calls.length
    store.mapZoom = layer.maxZoom! + 1
    expect(await store.getDistrictStatistics()).toEqual([])
    expect(calls.length).toBe(before)
  })
})

describe('dialogy a historie', () => {
  it('setDialog otevře panel, historie a krok zpět', async () => {
    const ctrl = controllerSpy()
    const store = useMapStore()
    await store.init('prague')

    await store.setDialog('Layers')
    expect(store.dialogType).toBe('Layers')
    expect(store.sideDialogOptions).toMatchObject({ position: 'right', width: '33%' })
    expect(ctrl.fitMapToWindow).toHaveBeenLastCalledWith(50, '67%')

    await store.setDialog('Search')
    await store.setDialog('Search') // duplicitní položka se do historie nepřidá
    expect(store.dialogHistory.map((a) => a.dialogType)).toEqual(['Layers', 'Search'])

    await store.setLastDialog()
    expect(store.dialogType).toBe('Layers')

    await store.setDialog('None')
    expect(store.dialogType).toBe('None')
    expect(store.dialogHistory).toEqual([])
    expect(store.sideDialogOptions).toBeNull()
    expect(ctrl.unselectAllSelectedPoints).toHaveBeenCalled()
    expect(ctrl.fitMapToWindow).toHaveBeenLastCalledWith(0, '100%')
  })

  it('mobilní panel je dole s výškou 50 %', async () => {
    controllerSpy()
    const store = useMapStore()
    store.setMobileView(true)
    await store.setDialog('Place', { places: [] })
    expect(store.sideDialogOptions).toMatchObject({ position: 'bottom', height: '50%', style: 'z-index:10000' })
  })

  it('nápověda na mobilu přes celou výšku okna, i při přepnutí z jiného panelu', async () => {
    controllerSpy()
    const store = useMapStore()
    store.setMobileView(true)
    store.windowHeight = 800
    await store.setDialog('Place', { places: [] })
    await store.setDialog('Help')
    expect(store.sideDialogOptions).toMatchObject({ position: 'bottom', height: '800px', style: 'z-index:50000' })
    store.setMobileView(false)
    expect(store.sideDialogOptions).toMatchObject({ position: 'right', width: '33%', height: '' })
  })
})

describe('vrstvy a časová osa z UI', () => {
  it('toggleLayer: WMS/polygony přepnou skupinu v mapě, objekty se znovu načtou, URL se aktualizuje', async () => {
    const ctrl = controllerSpy()
    const store = useMapStore()
    await store.init('prague')
    const layers = store.getNotBaseLayers()

    const inaccessible = layers.find((a) => a.name === 'inaccessible')!
    await store.toggleLayer(inaccessible)
    expect(inaccessible.selected).toBe(false)
    expect(ctrl.toggleLayerGroup).toHaveBeenLastCalledWith('inaccessible', false)

    const incident = layers.find((a) => a.name === 'incident')!
    const before = calls.filter((c) => c.url.endsWith('getmapobjects')).length
    await store.toggleLayer(incident)
    expect(calls.filter((c) => c.url.endsWith('getmapobjects')).length).toBe(before + 1)
    expect(new URLSearchParams(window.location.search).get('layers')).not.toContain('incident')
  })

  it('selectTimelinePoint: vybraný bod ignoruje, jinak přepne a zapíše do URL', async () => {
    controllerSpy()
    const store = useMapStore()
    await store.init('prague')
    const q2 = store.timeline!.find((a) => a.name === '1942_Q2')!
    await store.selectTimelinePoint(q2)
    expect(store.timelinePoint).toBe('1942-04-01')
    expect(new URLSearchParams(window.location.search).get('timeline')).toBe('1942_Q2')
    const callsBefore = calls.length
    await store.selectTimelinePoint(q2) // už vybraný → nic
    expect(calls.length).toBe(callsBefore)
  })
})

describe('getMapInfoForLeaflet', () => {
  it('vrstvy bez objektových + systémové skupiny', async () => {
    const store = useMapStore()
    await store.init('prague')
    const info = store.getMapInfoForLeaflet(15)
    expect(info.initialVariables?.zoom).toBe(15)
    expect(info.layers.some((a) => a.type === 'Objects' && a.name !== 'Objects')).toBe(false)
    expect(info.layers.slice(-3).map((a) => a.name)).toEqual(['Objects', 'AdditionalObjects', 'Narration'])
    expect(info.layers.find((a) => a.name === 'OSM')).toMatchObject({ type: 'Base', zIndex: 50 })
  })
})

describe('showPlaceInfo', () => {
  it('vybere body, otevře panel s místy a zapíše místo do URL', async () => {
    const ctrl = controllerSpy()
    const store = useMapStore()
    await store.init('prague')
    store.windowWidth = 1200
    apiResponses.getmapobjects = {
      $id: '1',
      $values: [{ $id: '2', placeType: 'Address', layerName: 'address', id: 699, citizens: 6, citizensTotal: 14, dateFrom: null }],
    }
    await store.showPlaceInfo(16, [{ x: 14.42, y: 50.08 }, { x: 14.43, y: 50.09 }], 1000)

    expect(store.dialogType).toBe('Place')
    expect(store.dialogParameters.places?.map((a) => a.id)).toEqual(['699'])
    expect(ctrl.selectPointOnMap).toHaveBeenCalledWith(['Address_699_'])
    expect(ctrl.goToLocation).toHaveBeenCalled() // klik v pravé třetině (pod panelem) → vycentrovat
    expect(ctrl.setUrlFromSelectedPlace).toHaveBeenCalledWith([{ x: 14.42, y: 50.08 }, { x: 14.42, y: 50.08 }])
  })
})
