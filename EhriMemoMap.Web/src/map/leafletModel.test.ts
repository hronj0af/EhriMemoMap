import { describe, expect, it } from 'vitest'
import { fromMapObject, fromPlace, fromSolrPlace, fromStatistics } from './leafletModel'
import type { MapObject, MapStatistic, Place, SolrPlace } from '../models/api'
import type { LayerModel } from '../models/map'

const mapObject = (patch: Partial<MapObject>): MapObject => ({
  placeType: 'Address', layerName: 'address', citizens: 6, citizensTotal: 14, id: 699,
  labelCs: 'Praha I, Štupartská 4', labelEn: 'Prague I, Štupartská 4',
  geographyMapPoint: null, geographyMapPolygon: null,
  mapPoint: '{"type":"Point","coordinates":[14.42,50.08]}', mapPolygon: null,
  dateFrom: '1942-04-01T00:00:00', dateTo: null,
  ...patch,
})

describe('fromMapObject', () => {
  it('ikona adresy podle podílu obětí', () => {
    const icon = (citizens: number | null, total: number | null) =>
      fromMapObject(mapObject({ citizens, citizensTotal: total }), false).htmlIcon
    expect(icon(14, 14)).toContain('victims_100_14.png')
    expect(icon(7, 14)).toContain('victims_051_7.png')
    expect(icon(6, 14)).toContain('victims_001_6.png')
    expect(icon(0, 14)).toContain('victims_000_0.png')
    expect(icon(null, 14)).toContain('victims_100.png')
  })

  it('poslední bydliště má invertovanou ikonu', () => {
    expect(fromMapObject(mapObject({ placeType: 'AddressLastResidence' }), false).htmlIcon).toContain('filter:invert(1);')
  })

  it('vlastnosti z vrstvy a popisek', () => {
    const layer = { name: 'Address', priorityOnMap: 3, customPolygonColor: '#123', labelCode: null } as LayerModel
    const result = fromMapObject(mapObject({}), false, [layer])
    expect(result).toMatchObject({
      clickable: true, id: '699', label: 'Praha I, Štupartská 4', priorityOnMap: 3, customPolygonColor: '#123',
      guid: 'Address_699_1942-04-01T00:00:00', iconAnchor: [12, 12],
    })
  })

  it('popisek z labelCode vrstvy', () => {
    const layer = { name: 'address', labelCode: 'present' } as LayerModel
    expect(fromMapObject(mapObject({}), false, [layer]).label).toBe('Přítomní')
  })

  it('heatmapa – jen bod a počet, neklikatelná', () => {
    const result = fromMapObject(mapObject({}), true)
    expect(result).toMatchObject({ heatmap: true, clickable: false, citizens: 6, guid: null, htmlIcon: null })
  })
})

describe('fromPlace', () => {
  const place = (type: string, narrativeMapId: number | null = 9): Place =>
    ({ id: 1, type, labelCs: 'a', labelEn: 'b', mapPoint: 'p', stopId: 2, narrativeMapId } as Place)

  it('hlavní bod – barevná ikona podle id příběhu', () => {
    expect(fromPlace(place('main point'), false).htmlIcon).toContain('narrative-icon2.png')
    expect(fromPlace(place('main point'), true).htmlIcon).toContain('narrative-icon.png')
    expect(fromPlace(place('main point')).clickable).toBe(true)
  })

  it('bod trajektorie a ostatní', () => {
    expect(fromPlace(place('trajectory point')).iconAnchor).toEqual([10, 10])
    expect(fromPlace(place('context point')).htmlIcon).toContain('marker-icon-wine-red.png')
  })
})

describe('fromSolrPlace', () => {
  it('červená ikona pro poslední bydliště', () => {
    expect(fromSolrPlace({ id: 'victim_last_residence_1', labelCs: 'x' } as SolrPlace).htmlIcon).toContain('marker-icon-red.png')
    expect(fromSolrPlace({ id: 'incident_1', labelCs: 'x' } as SolrPlace).htmlIcon).toContain('marker-icon.png')
  })
})

describe('fromStatistics', () => {
  it('tabulka s počty a polygon', () => {
    const stats = [
      { id: 1, type: 'victims', quarterCs: 'Praha I', quarterEn: 'Prague I', count: 120, mapPolygon: 'poly', mapPoint: null },
      { id: 2, type: 'incidents', quarterCs: 'Praha I', quarterEn: 'Prague I', count: 4, mapPolygon: null, mapPoint: 'pt' },
    ] as MapStatistic[]
    const result = fromStatistics(stats)!
    expect(result).toMatchObject({ placeType: 'Statistics', guid: 'Statistics_1_', mapPolygon: 'poly', mapPoint: 'pt', clickable: false })
    expect(result.label).toContain('Praha I')
    expect(result.label).toMatch(/120\s*<\/td>/)
    expect(fromStatistics([])).toBeNull()
  })
})
