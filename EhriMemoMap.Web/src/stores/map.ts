// Stav aplikace a logika mapy – přepis EhriMemoMap.Client/Services/MapService*.cs
// (MapServiceBase.cs, MapServiceLogic.cs, MapServiceStoryMap.cs) a [JSInvokable] handlerů z Pages/Map.razor.
//
// Rozdíly proti Blazoru:
// - NotifyStateChanged()/OnChange odpadá – komponenty reagují na změny stavu automaticky (Pinia/Vue reaktivita).
// - Volání JavaScriptu (IJSRuntime "mapAPI.*") jde přes mapController() (viz map/controller.ts).
// - DialogService.OpenSide/CloseSide nahrazuje stav dialogType + sideDialogOptions (vykreslí SideDialog.vue).
// - Načítání objektů při pohybu mapy ruší předchozí rozběhnutý požadavek (AbortController).
// - Velká data z API jsou ve shallowRef (nejsou hluboce reaktivní – nemění se, jen se vyměňují celá).
import { defineStore } from 'pinia'
import { computed, ref, shallowRef } from 'vue'
import { getConfig, type AppState } from '../config'
import { isAbortError } from '../api/client'
import * as api from '../api/endpoints'
import type {
  MapObject,
  MapObjectParameters,
  MapStatistic,
  NarrativeMap,
  Place,
  PlacesParameters,
  PlacesResult,
  PointModel,
  SolrPlace,
  SolrQueryParameters,
  TimelinePointParameter,
  VictimLongInfoModel,
  WelcomeDialogStatistics,
} from '../models/api'
import {
  DialogType,
  LayerType,
  MapState,
  MapType,
  ModalType,
  NarrativeMapStopPlaceType,
  PlaceType,
} from '../models/enums'
import type {
  Coordinate,
  DialogParameters,
  DialogParametersHistoryItem,
  LayerForLeaflet,
  LayerModel,
  MapModel,
  MapObjectForLeaflet,
  MapSettingsForLeaflet,
  SideDialogOptions,
  TimelinePointModel,
} from '../models/map'
import { loadMapSettings } from '../models/mapSettings'
import { dateKey } from '../models/helpers'
import { fromMapObject, fromPlace, fromStatistics } from '../map/leafletModel'
import { mapController } from '../map/controller'
import { setUrlParam } from '../utils/url'

/** Porovnání jako v C# s nullable int: s null je výsledek vždy false. */
function gte(a: number, b: number | null | undefined): boolean {
  return b !== null && b !== undefined && a >= b
}
function lte(a: number, b: number | null | undefined): boolean {
  return b !== null && b !== undefined && a <= b
}
function lt(a: number, b: number | null | undefined): boolean {
  return b !== null && b !== undefined && a < b
}
function gt(a: number, b: number | null | undefined): boolean {
  return b !== null && b !== undefined && a > b
}

function toPointModel(c: Coordinate | null): PointModel | null {
  return c ? { x: c.x, y: c.y } : null
}

/** Bod časové osy pro API – jen pole, která API čte (vnořené vrstvy by C# posílal s číselnými enumy). */
function toTimelineParameter(point: TimelinePointModel | null | undefined): TimelinePointParameter | null {
  if (!point) return null
  return { name: point.name, title: point.title, selected: point.selected, from: point.from, to: point.to }
}

function arePlacesEqual(a: MapObjectForLeaflet[] | null | undefined, b: MapObjectForLeaflet[] | null | undefined): boolean {
  if (!a && !b) return true
  if (!a || !b) return false
  if (a.length !== b.length) return false
  return a.every((p, i) => p.id === b[i].id)
}

export const useMapStore = defineStore('map', () => {
  // ------------------------------------------------------------------
  // STAV (MapServiceBase)
  // ------------------------------------------------------------------
  const appState = ref<AppState>(getConfig().appState)
  const mapState = ref<MapState>(MapState.NotInitialized)
  /** Nastavení mapy z mapsettings.{city}.json (MapService.Map) */
  const map = ref<MapModel | null>(null)

  const widthOfDialogRatio = 1 / 3
  const widthOfDialogPercent = Math.round(100 * widthOfDialogRatio) + '%'
  const heightOfDialog = 50

  const windowWidth = ref(0)
  const windowHeight = ref(0)
  const isMobileView = ref(false)
  const isDialogFullScreen = ref(false)

  const searchedPlaceString = ref('')
  const searchedPlaces = shallowRef<SolrPlace[] | null>(null)

  const narrativeMap = shallowRef<NarrativeMap | null>(null)
  const allNarrativeMaps = shallowRef<NarrativeMap[] | null>([])
  const victimLongInfo = shallowRef<VictimLongInfoModel | null>(null)

  const dialogType = ref<DialogType>(DialogType.None)
  const dialogParameters = shallowRef<DialogParameters>({})
  const dialogHistory = shallowRef<DialogParametersHistoryItem[]>([])
  /** Nastavení postranního panelu – určí se při jeho otevření (jako Radzen OpenSide). */
  const sideDialogOptions = ref<SideDialogOptions | null>(null)
  /** Modální dialog (Welcome na desktopu, Error při AppState Shutdown) */
  const modal = ref<ModalType>(ModalType.None)

  const mapType = ref<MapType>(MapType.Normal)
  const mapZoom = ref(0)
  const mapSouthWestPoint = ref<Coordinate | null>(null)
  const mapNorthEastPoint = ref<Coordinate | null>(null)

  const showLayersForce = ref(false)

  // příběhové mapy (MapServiceStoryMap)
  const expandedStopId = ref<number | null>(null)
  const expandedSections = ref(new Set<string>())

  /** Guid vybraných bodů – po překreslení mapy se znovu zvýrazní (Map.razor selectedPointGuidsJson) */
  const selectedPointGuids = ref<string[] | null>(null)

  /** Zrušení probíhajícího načítání objektů při dalším pohybu mapy */
  let refreshAbort: AbortController | null = null

  const city = computed(() => map.value?.initialVariables?.city ?? null)

  // ------------------------------------------------------------------
  // VRSTVY A ČASOVÁ OSA
  // ------------------------------------------------------------------

  /** Všechny vrstvy kromě podkladových (+ doplňkové vrstvy vybraného bodu časové osy) – GetNotBaseLayers */
  function getNotBaseLayers(onlySelected = false): LayerModel[] {
    const layers = map.value?.layers
    if (!layers || !layers.some((a) => a.type !== LayerType.Base)) return []

    const result = layers.filter((a) => a.type !== LayerType.Base && (!onlySelected || a.selected))

    const timeline = map.value?.timeline
    if (!timeline || !timeline.some((a) => a.selected && a.additionalLayers)) return result

    for (const point of timeline) {
      if (point.selected && point.additionalLayers) {
        result.push(...point.additionalLayers.filter((b) => !onlySelected || b.selected))
      }
    }
    return result
  }

  const timeline = computed(() => map.value?.timeline ?? null)

  /** Datum vybraného bodu časové osy (GetTimelinePoint) – yyyy-MM-dd, nebo null */
  const timelinePoint = computed<string | null>(() => {
    const point = map.value?.timeline?.find((a) => a.selected && a.from != null)
    return point ? dateKey(point.from) : null
  })

  /** ToggleTimeLinePoint – stejná logika jako C# (vybraný bod si ponechá stav, ostatní se odznačí, pak se přepne). */
  function toggleTimelinePoint(point: TimelinePointModel): void {
    const points = map.value?.timeline
    if (!points) return
    points.forEach((a) => (a.selected = point.name === a.name && point.selected))
    point.selected = !point.selected
  }

  /** Klik na bod časové osy (Timeline.razor OnCollectionClick): přepne bod, načte objekty a zapíše bod do URL. */
  async function selectTimelinePoint(point: TimelinePointModel | null | undefined): Promise<void> {
    if (!point || point.selected) return
    toggleTimelinePoint(point)
    await refreshObjectsOnMap(false)
    setUrlParam('timeline', getTimelineForUrlParameter())
  }

  /**
   * Zapnutí/vypnutí vrstvy (Layers.razor a LayersQuickAccessButtons.razor OnLayerClick).
   * Heatmapy se vylučují navzájem. WMS/polygonové/WFS vrstvy se jen skryjí/zobrazí v mapě,
   * u objektových vrstev a heatmap se znovu načtou objekty. Výběr vrstev se zapíše do URL.
   * Pozn.: rychlá tlačítka v Blazoru u polygonových vrstev volala toggleLayerGroup("Polygons"), což
   * v site.ts skončilo výjimkou (skupina toho jména neexistuje) – tady se používá název vrstvy jako v dialogu.
   */
  async function toggleLayer(layer: LayerModel): Promise<void> {
    layer.selected = !layer.selected

    if (layer.type === LayerType.Heatmap && layer.selected) {
      for (const other of getNotBaseLayers()) {
        if (other.type === LayerType.Heatmap && other.name !== layer.name) other.selected = false
      }
    }

    if (layer.type === LayerType.WMS || layer.type === LayerType.Polygons || layer.type === LayerType.WFS) {
      mapController().toggleLayerGroup(layer.name ?? '', layer.selected)
    } else if (layer.type === LayerType.Objects || layer.type === LayerType.Heatmap) {
      await refreshObjectsOnMap(false)
    }

    setUrlParam('layers', getLayersForUrlParameter())
  }

  /** Názvy vybraných vrstev pro URL parametr "layers" (GetLayersForUrlParameter) */
  function getLayersForUrlParameter(): string {
    return getNotBaseLayers()
      .filter((a) => a.selected)
      .map((a) => a.name)
      .join(',')
  }

  /** Název vybraného bodu časové osy pro URL parametr "timeline" (GetTimelineForUrlParameter) */
  function getTimelineForUrlParameter(): string | null {
    return map.value?.timeline?.find((a) => a.selected)?.name ?? null
  }

  function setShowLayersForce(value: boolean): void {
    showLayersForce.value = value
    mapController().requestRefresh()
  }

  // ------------------------------------------------------------------
  // INICIALIZACE
  // ------------------------------------------------------------------

  /** Init – načte mapsettings.{city}.json, nastaví vybrané vrstvy a bod časové osy (z URL nebo výchozí). */
  async function init(cityName: string, layers?: string | null, timelinePointName?: string | null): Promise<boolean> {
    const settings = await loadMapSettings(cityName)
    if (!settings) return false
    if (settings.initialVariables) settings.initialVariables.heightOfDialog = heightOfDialog
    map.value = settings

    initInfoAboutLayersSelection(layers ? layers.split(',') : null)
    initInfoAboutTimeline(timelinePointName ?? null)

    mapState.value = MapState.Initialized
    return true
  }

  /** InitInfoAboutLayersSelection */
  function initInfoAboutLayersSelection(layers: string[] | null): void {
    const m = map.value
    if (!m) return
    m.layers?.forEach((layer) => {
      layer.selected =
        layer.type === LayerType.Base ||
        (layers === null && layer.type !== LayerType.Heatmap) ||
        (layers?.includes(layer.name ?? '') ?? false)
    })
    m.timeline
      ?.flatMap((a) => a.additionalLayers ?? [])
      .forEach((layer) => {
        layer.selected = layers === null || layers.includes(layer.name ?? '')
      })
  }

  /** InitInfoAboutTimeline */
  function initInfoAboutTimeline(timelinePointName: string | null): void {
    map.value?.timeline?.forEach((point) => {
      point.selected = (!timelinePointName && point.from == null) || point.name === timelinePointName
    })
  }

  /** Destroy – úklid při odchodu z mapy (na úvodní stránku). */
  function destroy(): void {
    refreshAbort?.abort()
    refreshAbort = null
    mapState.value = MapState.NotInitialized
    map.value = null
    narrativeMap.value = null
    victimLongInfo.value = null
    dialogHistory.value = []
    dialogParameters.value = {}
    dialogType.value = DialogType.None
    sideDialogOptions.value = null
    mapType.value = MapType.Normal
    allNarrativeMaps.value = null
    selectedPointGuids.value = null
    expandedStopId.value = null
    expandedSections.value.clear()
    mapController().destroyMap()
    modal.value = ModalType.None
  }

  /** Nastavení pro Leaflet – podkladové a WMS vrstvy + skupiny objektů (GetMapInfoForLeaflet) */
  function getMapInfoForLeaflet(zoom = 0): MapSettingsForLeaflet {
    const m = map.value
    if (zoom > 0 && m?.initialVariables) m.initialVariables.zoom = zoom

    const layers: LayerForLeaflet[] = (m?.layers ?? [])
      .filter((a) => a.type !== LayerType.Objects)
      .map((a) => ({
        name: a.name,
        selected: a.selected,
        attribution: a.attribution,
        url: a.url,
        type: a.type,
        mapParameter: a.mapParameter,
        layersParameter: a.layersParameter,
        cssClass: a.cssClass ?? '',
        zIndex: a.zIndex,
      }))

    layers.push(
      { name: LayerType.Objects, type: LayerType.Objects, selected: true, zIndex: 0 },
      { name: 'AdditionalObjects', type: LayerType.Polygons, selected: true, zIndex: 9999 },
      { name: LayerType.Narration, type: LayerType.Narration, selected: false, zIndex: 9999 },
    )

    return { initialVariables: m?.initialVariables ?? null, layers }
  }

  // ------------------------------------------------------------------
  // DIALOGY
  // ------------------------------------------------------------------

  /** GetDialogOptions – rozměry a pozice postranního panelu pro mobil / desktop */
  function getDialogOptions(): SideDialogOptions {
    const mobile = isMobileView.value
    const fullHeight = dialogType.value === DialogType.Help || dialogType.value === DialogType.Welcome
    return {
      position: mobile ? 'bottom' : 'right',
      cssClass: mobile ? '' : 'side-dialog',
      style: mobile ? (fullHeight ? 'z-index:50000' : 'z-index:10000') : '',
      height: mobile ? (fullHeight ? windowHeight.value + 'px' : heightOfDialog + '%') : '',
      width: !mobile ? widthOfDialogPercent : '',
    }
  }

  /** AddToDialogHistory */
  function addToDialogHistory(type: DialogType, parameters?: DialogParameters | null): void {
    if (type === DialogType.None) {
      dialogHistory.value = []
      return
    }
    const last = dialogHistory.value[dialogHistory.value.length - 1]
    if (
      last &&
      type === last.dialogType &&
      (last.parameters.id ?? null) === (parameters?.id ?? null) &&
      arePlacesEqual(last.parameters.places, parameters?.places)
    ) {
      return
    }
    dialogHistory.value = [
      ...dialogHistory.value,
      {
        dialogType: type,
        mapType: mapType.value,
        victimInfo: victimLongInfo.value,
        narrativeMap: narrativeMap.value,
        parameters: { id: parameters?.id ?? null, places: parameters?.places ?? null },
      },
    ]
  }

  /** SetLastDialog – tlačítko "zpět" v dialogu */
  async function setLastDialog(): Promise<void> {
    if (dialogHistory.value.length <= 1) return
    const history = dialogHistory.value.slice(0, -1)
    dialogHistory.value = history
    const lastItem = history[history.length - 1]
    victimLongInfo.value = lastItem.victimInfo
    narrativeMap.value = lastItem.narrativeMap
    await setMapType(lastItem.mapType)
    await setDialog(lastItem.dialogType, lastItem.parameters)
  }

  /** SetDialog – otevře/přepne/zavře postranní panel */
  async function setDialog(newDialogType: DialogType, parameters?: DialogParameters | null): Promise<void> {
    if (dialogType.value === DialogType.None && newDialogType === DialogType.None) return

    const ctrl = mapController()
    if (newDialogType === DialogType.None) {
      ctrl.closeGallery()
      ctrl.removeAdditionalObjects()
      ctrl.unselectAllSelectedPoints()
      selectedPointGuids.value = null
      sideDialogOptions.value = null
    }

    const height = newDialogType === DialogType.Welcome || newDialogType === DialogType.None ? 0 : heightOfDialog
    ctrl.fitMapToWindow(height, isMobileView.value ? '100%' : newDialogType !== DialogType.None ? '67%' : '100%')

    // Radzen OpenSide: volby se počítají při otevření panelu, ještě s původním (None) typem dialogu
    if (dialogType.value === DialogType.None && newDialogType !== DialogType.None) {
      sideDialogOptions.value = getDialogOptions()
    }

    dialogType.value = newDialogType
    dialogParameters.value = parameters ?? {}

    addToDialogHistory(newDialogType, parameters)
  }

  /** SetMapType – přepínání běžné mapy a příběhových map */
  async function setMapType(newValue: MapType): Promise<void> {
    if (newValue === mapType.value) return
    const ctrl = mapController()

    if (newValue === MapType.Normal) {
      ctrl.resetMapViewToInitialState()
      ctrl.showLayersForNormalMap()
      narrativeMap.value = null
    } else {
      ctrl.hideLayersForNormalMap()
      if (newValue === MapType.AllStoryMaps) showAllNarrativeMapsPlaces()
      else showNarrativeMapPlaces()
    }

    if (isMobileView.value) ctrl.toggleScaleVisibility(newValue === MapType.Normal)

    mapType.value = newValue
  }

  /** ShouldBeMapCenteredAfterClick – posunout mapu, aby místo nezakryl panel? */
  function shouldBeMapCenteredAfterClick(mouseClickPointX: number): boolean {
    if (isMobileView.value) return true
    return Math.trunc(mouseClickPointX) > windowWidth.value - windowWidth.value * widthOfDialogRatio
  }

  function setBBox(coordinates: Coordinate[] | null | undefined): void {
    if (!coordinates || coordinates.length !== 2) return
    mapSouthWestPoint.value = coordinates[0]
    mapNorthEastPoint.value = coordinates[1]
  }

  function setMobileView(value: boolean): void {
    isMobileView.value = value
  }

  // ------------------------------------------------------------------
  // OBJEKTY NA MAPĚ (MapServiceLogic)
  // ------------------------------------------------------------------

  /** RefreshObjectsOnMap – načte objekty, statistiky a heatmapu pro aktuální výřez a předá je mapě */
  async function refreshObjectsOnMap(withPolygons: boolean): Promise<void> {
    refreshAbort?.abort()
    const abort = new AbortController()
    refreshAbort = abort

    try {
      // v C# se volalo postupně, nezávislé požadavky tu běží paralelně
      const [objects, statistics, heatmap] = await Promise.all([
        getMapObjects(withPolygons, true, null, abort.signal),
        getDistrictStatistics(abort.signal),
        getHeatMap(null, abort.signal),
      ])
      if (abort.signal.aborted) return
      mapController().refreshObjectsOnMap([...objects, ...statistics, ...heatmap])
    } catch (error) {
      if (isAbortError(error)) return
      console.error('Načtení objektů na mapě selhalo', error)
    } finally {
      if (refreshAbort === abort) refreshAbort = null
    }
  }

  /** GetMapObjects – objekty vybraných vrstev ve výřezu mapy (nebo v okolí zadaných souřadnic) */
  async function getMapObjects(
    withPolygons: boolean,
    aggregate: boolean,
    customCoordinates?: Coordinate[] | null,
    signal?: AbortSignal,
  ): Promise<MapObjectForLeaflet[]> {
    const m = map.value
    if (!m) return []

    const selectedTimelinePoint = m.timeline?.find((a) => a.selected)

    const zoom = mapZoom.value
    const selectedLayerNames: string[] = []
    const layers = getNotBaseLayers(true)
      .filter((a) => a.type !== LayerType.Heatmap && a.type !== LayerType.WFS)
      .filter((a) => a.placeType != null && (withPolygons || a.type !== LayerType.Polygons))

    for (const layer of layers) {
      // vrstva se zobrazuje jen v rozsahu svého zoomu (pokud to není vynucené)
      if (!showLayersForce.value && (lt(zoom, layer.minZoom) || gt(zoom, layer.maxZoom))) continue
      selectedLayerNames.push(layer.placeType!)
    }

    const parameters: MapObjectParameters = {
      city: m.initialVariables?.city ?? null,
      selectedTimeLinePoint: toTimelineParameter(selectedTimelinePoint),
      selectedLayerNames,
      customCoordinates: customCoordinates?.map((a) => ({ x: a.x, y: a.y })) ?? null,
      mapSouthWestPoint: toPointModel(mapSouthWestPoint.value),
      mapNorthEastPoint: toPointModel(mapNorthEastPoint.value),
    }

    const [apiObjects, wfsObjects] = await Promise.all([
      api.getMapObjects(parameters, { signal }),
      withPolygons ? getWfsObjects(signal) : Promise.resolve([] as MapObject[]),
    ])

    let objects = apiObjects ?? []
    if (aggregate) objects = aggregateSameAddresses(objects)
    objects = objects.concat(wfsObjects)

    return objects.map((a) => fromMapObject(a, false, m.layers))
  }

  /** GetWFSObjects – objekty z vybraných WFS vrstev */
  async function getWfsObjects(signal?: AbortSignal): Promise<MapObject[]> {
    const layers = getNotBaseLayers().filter((a) => a.type === LayerType.WFS && a.selected)
    if (layers.length === 0) return []
    const result = await api.getWfsObjects(
      { wfsLayers: layers.map((layer) => ({ name: layer.name, url: layer.url, placeType: layer.placeType })) },
      { signal },
    )
    return result ?? []
  }

  /**
   * AggregateSameAddresses – sloučí objekty stejného typu se stejnými souřadnicemi do jednoho
   * (adresy, u kterých se nepodařilo dohledat přesné souřadnice).
   */
  function aggregateSameAddresses(objects: MapObject[] | null): MapObject[] {
    const result: MapObject[] = []
    if (!objects) return result

    const sorted = [...objects].sort((a, b) => compareOrdinalNullable(a.labelCs, b.labelCs))
    const byPlaceType = groupBy(sorted, (a) => a.placeType ?? '')

    for (const group of byPlaceType.values()) {
      const byCoordinates = groupBy(group, (a) => {
        const c = a.geographyMapPoint?.coordinates as number[] | undefined
        return c ? `${c[0]}|${c[1]}` : '|'
      })
      for (const same of byCoordinates.values()) {
        if (same.length === 1) {
          result.push(same[0])
          continue
        }
        const sum = (values: (number | null)[]) =>
          values.some((v) => v != null) ? values.reduce<number>((acc, v) => acc + (v ?? 0), 0) : 0
        const distinct = (values: (string | null)[]) => [...new Set(values)].join(' <br/> ')
        result.push({
          ...same[0],
          citizens: sum(same.map((a) => a.citizens)),
          citizensTotal: sum(same.map((a) => a.citizensTotal)),
          labelCs: distinct(same.map((a) => a.labelCs)),
          labelEn: distinct(same.map((a) => a.labelEn)),
        })
      }
    }
    return result
  }

  /** GetDistrictStatistics – statistiky městských částí (polygony s tabulkou) */
  async function getDistrictStatistics(signal?: AbortSignal): Promise<MapObjectForLeaflet[]> {
    const layer = getNotBaseLayers().find((a) => a.selected && a.placeType === PlaceType.Statistics)
    if (!layer) return []

    const zoom = mapZoom.value
    const inRange = gte(zoom, layer.minZoom) && lte(zoom, layer.maxZoom)
    if (!inRange && !(lt(zoom, layer.minZoom) && zoom >= 0)) return []

    const statistics: MapStatistic[] =
      (await api.getDistrictStatistics(
        { city: map.value?.initialVariables?.city ?? null, total: !inRange, timeLinePoint: timelinePoint.value },
        { signal },
      )) ?? []

    return [...groupBy(statistics, (a) => a.quarterCs ?? '').values()]
      .map((group) => fromStatistics(group))
      .filter((a): a is MapObjectForLeaflet => a !== null)
  }

  /** GetHeatMap – data pro heatmapu, pokud je vrstva heatmapy vybraná */
  async function getHeatMap(customCoordinates?: Coordinate[] | null, signal?: AbortSignal): Promise<MapObjectForLeaflet[]> {
    const layer = getNotBaseLayers().find((a) => a.selected && a.type === LayerType.Heatmap)
    if (!layer) return []

    const parameters: MapObjectParameters = {
      selectedLayerNames: [layer.placeType ?? null],
      selectedTimeLinePoint: { from: null, to: null },
      customCoordinates: customCoordinates?.map((a) => ({ x: a.x, y: a.y })) ?? null,
      mapSouthWestPoint: toPointModel(mapSouthWestPoint.value),
      mapNorthEastPoint: toPointModel(mapNorthEastPoint.value),
      city: map.value?.initialVariables?.city ?? null,
    }
    const heatmap = (await api.getHeatmap(parameters, { signal })) ?? []
    return heatmap.map((a) => fromMapObject(a, true))
  }

  function getWelcomeDialogStatistics(): Promise<WelcomeDialogStatistics | null> {
    return api.getWelcomeDialogStatistics(city.value)
  }

  function getPlaces(parameters: PlacesParameters): Promise<PlacesResult | null> {
    return api.getPlaces(parameters)
  }

  async function getSolrPlaces(parameters: SolrQueryParameters): Promise<SolrPlace[]> {
    return (await api.getSolrPlaces(parameters)) ?? []
  }

  async function getVictimLongInfo(id: number | null | undefined): Promise<VictimLongInfoModel | null> {
    if (id == null) return null
    return api.getVictimLongInfo(city.value, id)
  }

  /** ShowVictim – detail oběti v panelu */
  async function showVictim(id: number | null | undefined): Promise<void> {
    victimLongInfo.value = await getVictimLongInfo(id)
    await setDialog(DialogType.Victim, { id: victimLongInfo.value?.id ?? null, places: dialogParameters.value.places })
  }

  // ------------------------------------------------------------------
  // HANDLERY Z MAPY (Map.razor [JSInvokable])
  // ------------------------------------------------------------------

  /** RefreshObjectsOnMap volané mapou po posunu/zoomu */
  async function onMapMoved(withPolygons: boolean, zoom: number, bounds: [Coordinate, Coordinate]): Promise<void> {
    mapZoom.value = zoom
    setBBox(bounds)
    await refreshObjectsOnMap(withPolygons)
    if (selectedPointGuids.value?.length) mapController().selectPointOnMap(selectedPointGuids.value)
  }

  /** ShowPlaceInfo – klik na místo v mapě: zvýrazní body a otevře panel s informacemi */
  async function showPlaceInfo(_zoom: number, coordinates: Coordinate[], mouseClickPointX: number): Promise<void> {
    let placesOnMap: MapObjectForLeaflet[]
    try {
      placesOnMap = await getMapObjects(true, false, coordinates)
    } catch (error) {
      console.error('Načtení míst v okolí kliknutí selhalo', error)
      return
    }
    if (placesOnMap.length === 0) return

    const ctrl = mapController()
    selectedPointGuids.value = placesOnMap.map((a) => a.guid ?? '')
    ctrl.selectPointOnMap(selectedPointGuids.value)

    await setDialog(DialogType.Place, { places: placesOnMap })

    if (shouldBeMapCenteredAfterClick(mouseClickPointX)) ctrl.goToLocation(coordinates[0], mapZoom.value)

    ctrl.setUrlFromSelectedPlace([
      { x: coordinates[0].x, y: coordinates[0].y },
      { x: coordinates[0].x, y: coordinates[0].y },
    ])
  }

  // ------------------------------------------------------------------
  // PŘÍBĚHOVÉ MAPY (MapServiceStoryMap)
  // ------------------------------------------------------------------

  /** ShowPlacesOnMap – zobrazí místa příběhové mapy ve vrstvě Narration */
  function showPlacesOnMap(places: Place[] | null | undefined, defaultColor = true): void {
    if (!places || places.length === 0) return
    const transformed = places.map((b) => fromPlace(b, defaultColor))

    // poslední bod trajektorie dostane ikonu příběhu a je klikatelný
    for (let i = transformed.length - 1; i >= 0; i--) {
      if (transformed[i].placeType === NarrativeMapStopPlaceType.Trajectory) {
        transformed[i].htmlIcon = "<img src='css/images/narrative-icon.png' />"
        transformed[i].iconAnchor = [22, 55]
        transformed[i].clickable = true
        break
      }
    }
    mapController().addObjects(transformed, LayerType.Narration)
  }

  async function showNarrativeMap(id: number | null | undefined): Promise<void> {
    if (id == null) return
    await setMapType(MapType.StoryMapWhole)
    await getNarrativeMap(id, city.value)
    await setDialog(DialogType.StoryMap, { id })
    showNarrativeMapPlaces()
  }

  async function showAllStoryMaps(): Promise<void> {
    dialogParameters.value = { ...dialogParameters.value, id: null }
    narrativeMap.value = null
    await setMapType(MapType.AllStoryMaps)
    showAllNarrativeMapsPlaces()
    addToDialogHistory(DialogType.StoryMap, { id: null })
  }

  function showNarrativeMapPlaces(): void {
    const nm = narrativeMap.value
    if (!nm) return
    showPlacesOnMap(
      (nm.stops ?? []).flatMap((a) => a.places ?? []).filter((a) => a.type === NarrativeMapStopPlaceType.Main),
    )
  }

  function showAllNarrativeMapsPlaces(): void {
    if (!allNarrativeMaps.value) return
    showPlacesOnMap(
      allNarrativeMaps.value.flatMap((a) => a.mainPoints ?? []),
      false,
    )
  }

  async function getAllNarrativeMaps(): Promise<void> {
    if (!(map.value?.initialVariables?.storyMaps ?? false)) return
    if (allNarrativeMaps.value && allNarrativeMaps.value.length > 0) return
    allNarrativeMaps.value = (await api.getAllNarrativeMaps(city.value)) ?? []
  }

  async function getNarrativeMap(id: number | null | undefined, cityName: string | null): Promise<void> {
    if (id == null) return
    narrativeMap.value = await api.getNarrativeMap(id, cityName)
  }

  async function showStopPlacesOnMap(stopId: number): Promise<void> {
    await setMapType(MapType.StoryMapOneStop)
    const stop = narrativeMap.value?.stops?.find((a) => a.id === stopId)
    if (!stop) return
    showPlacesOnMap(stop.places)
  }

  /** SwitchStoryMap – tlačítko příběhových map */
  async function switchStoryMap(): Promise<void> {
    if (dialogType.value === DialogType.StoryMap) {
      await setMapType(MapType.Normal)
      await setDialog(DialogType.None)
      return
    }

    if (allNarrativeMaps.value && allNarrativeMaps.value.length === 1) {
      const id = allNarrativeMaps.value[0]?.id ?? null
      dialogParameters.value = { ...dialogParameters.value, id }
      await showNarrativeMap(id)
    } else {
      dialogParameters.value = { ...dialogParameters.value, id: null }
      narrativeMap.value = null
      await setMapType(MapType.AllStoryMaps)
      showAllNarrativeMapsPlaces()
      await setDialog(DialogType.StoryMap)
    }
  }

  /** OpenStoryMapTimelineLabel – klik na zastávku (v mapě nebo na časové ose příběhu) */
  async function openStoryMapTimelineLabel(stopId: number, narrativeMapId: number | null): Promise<void> {
    if (!narrativeMap.value || narrativeMap.value.id !== narrativeMapId) {
      // není otevřená správná příběhová mapa – otevřít
      await getNarrativeMap(narrativeMapId, city.value)
      await showNarrativeMap(narrativeMapId)
    }

    expandedSections.value.clear()
    if (expandedStopId.value === stopId) {
      // klik na už otevřenou zastávku ji zavře
      expandedStopId.value = null
      showNarrativeMapPlaces()
    } else {
      expandedStopId.value = stopId
      await showStopPlacesOnMap(stopId)
    }
  }

  return {
    // stav
    appState,
    mapState,
    map,
    city,
    widthOfDialogRatio,
    widthOfDialogPercent,
    heightOfDialog,
    windowWidth,
    windowHeight,
    isMobileView,
    isDialogFullScreen,
    searchedPlaceString,
    searchedPlaces,
    narrativeMap,
    allNarrativeMaps,
    victimLongInfo,
    dialogType,
    dialogParameters,
    dialogHistory,
    sideDialogOptions,
    modal,
    mapType,
    mapZoom,
    mapSouthWestPoint,
    mapNorthEastPoint,
    showLayersForce,
    expandedStopId,
    expandedSections,
    selectedPointGuids,
    timeline,
    timelinePoint,
    // vrstvy, časová osa
    getNotBaseLayers,
    toggleTimelinePoint,
    selectTimelinePoint,
    toggleLayer,
    getLayersForUrlParameter,
    getTimelineForUrlParameter,
    setShowLayersForce,
    // inicializace
    init,
    destroy,
    getMapInfoForLeaflet,
    // dialogy
    getDialogOptions,
    addToDialogHistory,
    setLastDialog,
    setDialog,
    setMapType,
    shouldBeMapCenteredAfterClick,
    setBBox,
    setMobileView,
    // objekty
    refreshObjectsOnMap,
    getMapObjects,
    aggregateSameAddresses,
    getDistrictStatistics,
    getHeatMap,
    getWelcomeDialogStatistics,
    getPlaces,
    getSolrPlaces,
    getVictimLongInfo,
    showVictim,
    // handlery z mapy
    onMapMoved,
    showPlaceInfo,
    // příběhové mapy
    showPlacesOnMap,
    showNarrativeMap,
    showAllStoryMaps,
    showNarrativeMapPlaces,
    showAllNarrativeMapsPlaces,
    getAllNarrativeMaps,
    getNarrativeMap,
    showStopPlacesOnMap,
    switchStoryMap,
    openStoryMapTimelineLabel,
  }
})

// ---------- pomocné funkce ----------

function groupBy<T>(items: T[], key: (item: T) => string): Map<string, T[]> {
  const result = new Map<string, T[]>()
  for (const item of items) {
    const k = key(item)
    const group = result.get(k)
    if (group) group.push(item)
    else result.set(k, [item])
  }
  return result
}

/** Řazení jako LINQ OrderBy na string? – null jako první, jinak řetězcové porovnání. */
function compareOrdinalNullable(a: string | null, b: string | null): number {
  if (a === b) return 0
  if (a === null) return -1
  if (b === null) return 1
  return a.localeCompare(b, 'cs')
}
