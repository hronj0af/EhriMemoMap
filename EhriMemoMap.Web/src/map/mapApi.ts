// Ovládání Leaflet mapy – přepis EhriMemoMap.Client/wwwroot/js/site.ts (namespace mapAPI) na ES modul.
//
// Rozdíly proti site.ts:
// - Data se předávají jako objekty, ne jako JSON řetězce (odpadá JSON.stringify/parse na hranici .NET ↔ JS).
// - Volání do Blazoru (invokeMethodAsync) nahrazují MapEventHandlers (viz map/controller.ts).
// - Zavírání tooltipů kliknutím do mapy řeší jeden posluchač; site.ts přidával mapě nový posluchač
//   "click" za každý marker a polygon při každém překreslení, takže jejich počet rostl s každým posunem mapy.
// - Odstraňované objekty se odebírají i ze skupiny (removeLayer), ne jen z mapy (layer.remove()).
// - Posluchač změny velikosti okna se registruje při inicializaci mapy a ruší při jejím zničení.
// - Zápis do URL zachovává history.state (vue-router).
import { L, HeatmapOverlay, type HeatmapData, type HeatmapOverlayLayer } from './plugins'
import type { MapController, MapEventHandlers } from './controller'
import type { Coordinate, InitialVariables, LayerForLeaflet, MapObjectForLeaflet, MapSettingsForLeaflet, TrajectoryStopForLeaflet } from '../models/map'
import { LayerType, NarrativeMapStopPlaceType } from '../models/enums'
import { setUrlParam, setUrlFromSelectedPlace as writeSelectedPlaceToUrl, getUrlParam } from '../utils/url'
import { closeGallery, isMobileView, resizePhotoHeight } from '../utils/dom'

// ------------------------------------------------------------------
// TYPY – vlastní volby skupin a markerů (Leaflet je uchovává v options)
// ------------------------------------------------------------------

interface GroupOptions extends L.LayerOptions {
  id: string
  type: string | null
  selected: boolean | null
  name: string | null
}
type Group = L.FeatureGroup & { options: GroupOptions }

interface MarkerExtraOptions extends L.MarkerOptions {
  guid?: string | null
  stopId?: number | null
  narrativeMapId?: number | null
  type?: string | null
}
type AppMarker = L.Marker & { options: MarkerExtraOptions }

interface PolygonExtraOptions extends L.PolylineOptions {
  originalFillColor?: string
}

interface GeoJsonPoint {
  type: string
  coordinates: number[]
}
interface GeoJsonPolygon {
  type: string
  coordinates: unknown[][]
}

// ------------------------------------------------------------------
// STAV
// ------------------------------------------------------------------

let map: L.Map | null = null
let mapSettings: MapSettingsForLeaflet | null = null
let initialVariables: InitialVariables | null = null
let mobileDialogHeight: number | null = null
let groups: Group[] = []
let mapInitialized = false
let handlers: MapEventHandlers | null = null

// poslední pozice myši nad mapou (pro klik na polygon a pozici panelu)
let lat = 0
let lng = 0
let coorx = 0

let bluepointIcon: L.Icon | null = null
let watchId: number | null = null
let applicationIsTrackingLocation = false
let actualLocation: GeolocationPosition | null = null

let isFullscreen = false

let heatmapLayer: HeatmapOverlayLayer | null = null
let heatmapData: { id: string; heatmapdata: HeatmapData }[] | null = null

const polygonColorDefault = '#222'
const polygonFillColorDefault = '#C5222C'
const polygonColorSelected = '#000'

/** Objekt, na který se právě kliklo – posluchač kliknutí na mapu jeho tooltip nezavře */
let tooltipClick: { layer: L.Layer; event: Event } | null = null

/** Otevře tooltip po kliknutí/klepnutí (polygon v místě klepnutí) a označí ho, aby ho mapa hned nezavřela. */
function openTooltipOnClick(layer: L.Marker | L.Polygon, e: L.LeafletMouseEvent, latlng?: L.LatLng): void {
  if (!layer.getTooltip()) return
  layer.openTooltip(latlng)
  tooltipClick = { layer, event: e.originalEvent }
}

function findGroup(id: string): Group | undefined {
  return groups.find((a) => a.options.id === id)
}

// ------------------------------------------------------------------
// INIT
// ------------------------------------------------------------------

export function setMapEventHandlers(value: MapEventHandlers | null): void {
  handlers = value
}

/** Připraví mapu do úvodního stavu (initMap). */
export function initMap(settings: MapSettingsForLeaflet): void {
  if (map) destroyMap()

  mapSettings = settings
  initialVariables = settings.initialVariables
  mobileDialogHeight = initialVariables?.heightOfDialog ?? null

  bluepointIcon = new L.Icon({
    iconUrl: 'images/blue-point.png',
    iconSize: [20, 20],
    iconAnchor: [10, 10],
  })

  const iv = initialVariables
  map = new L.Map('map', {
    zoomControl: false,
    maxZoom: iv?.maxZoom,
    minZoom: iv?.minZoom,
    // pozor: site.ts předává [x, y] jako [lat, lng] – zachováno (hodnoty jsou ±90, takže to nevadí)
    maxBounds:
      iv?.minBounds && iv.maxBounds
        ? [
            [iv.minBounds.x, iv.minBounds.y],
            [iv.maxBounds.x, iv.maxBounds.y],
          ]
        : undefined,
    zoomSnap: isMobileView() ? 0.1 : 1,
  })
  map.attributionControl.setPosition('bottomleft')
  L.control.scale().setPosition(isMobileView() ? 'topright' : 'bottomleft').addTo(map)

  if (!setMapWithInfoFromUrl()) resetMapViewToInitialState()

  // po posunu mapy aktualizovat URL a objekty
  map.on('moveend', () => {
    if (!mapInitialized) return
    const el = document.getElementById('map')
    if (el) el.style.cursor = 'default'
    setUrlByMapInfo()
    callRefreshObjectsOnMap()
  })

  // pozice myši pro klik na polygon (site.ts: mousemove → lat, lng, coorx)
  map.on('mousemove', (ev: L.LeafletMouseEvent) => {
    lat = ev.latlng.lat
    lng = ev.latlng.lng
    coorx = ev.containerPoint.x
  })

  // jeden posluchač pro zavření tooltipů po kliknutí mimo objekt. Kliknutí na objekt dostane nejdřív objekt
  // (otevře svůj tooltip) a pak mapa – tooltip objektu, na který se právě kliklo, se proto nezavírá
  // (jinak by se na mobilu, kde není "najetí myší", tooltip nikdy neukázal – např. statistiky čtvrtí).
  map.on('click', (e: L.LeafletMouseEvent) => {
    const keep = tooltipClick && tooltipClick.event === e.originalEvent ? tooltipClick.layer : null
    tooltipClick = null
    for (const group of groups) {
      group.eachLayer((layer) => {
        if (layer !== keep) (layer as L.Layer).closeTooltip?.()
      })
    }
  })

  // skupiny vrstev podle nastavení
  for (const layerSettings of settings.layers) {
    const group = new L.FeatureGroup(undefined, {
      id: layerSettings.name + '_group',
      type: layerSettings.type,
      selected: layerSettings.selected,
      name: layerSettings.name,
    } as GroupOptions) as Group
    groups.push(group)

    if (layerSettings.selected) group.addTo(map)

    const layer = convertMapSettingsObjectToMapLayer(layerSettings)
    if (layer) group.addLayer(layer)

    if (layerSettings.zIndex != null) group.setZIndex(layerSettings.zIndex)
  }

  window.addEventListener('resize', onResizeWindow)

  fitMapToWindow()
  mapInitialized = true
}

export function destroyMap(): void {
  window.removeEventListener('resize', onResizeWindow)
  hideMyLocation()

  if (heatmapLayer && map) map.removeLayer(heatmapLayer)
  heatmapLayer = null
  heatmapData = null

  for (const group of groups) {
    group.clearLayers()
    map?.removeLayer(group)
  }
  groups = []

  if (map) {
    map.off()
    map.remove()
    map = null
  }

  applicationIsTrackingLocation = false
  actualLocation = null
  initialVariables = null
  mapSettings = null
  mapInitialized = false
  isFullscreen = false
}

export function resetMapViewToInitialState(): void {
  if (!map) return
  if (initialVariables == null) {
    map.setView([50.07905886, 14.43715096], 14) // výchozí pohled na Prahu
    return
  }
  const mobile = isMobileView()
  const zoom = mobile ? initialVariables.zoomMobile : initialVariables.zoom
  const initialLat = mobile ? initialVariables.latMobile : initialVariables.lat
  const initialLng = mobile ? initialVariables.lngMobile : initialVariables.lng
  if (initialLat == null || initialLng == null || zoom == null) return
  map.setView([initialLat, initialLng], zoom)
}

/**
 * Změna velikosti okna (otočení telefonu, schování adresního řádku…). Rozměry mapy přepočítá aplikace
 * podle otevřeného panelu (handlers.windowResized → store.onWindowResize). site.ts tu volal fitMapToWindow()
 * bez parametrů, takže se mapa na mobilu natáhla i pod otevřený panel.
 */
function onResizeWindow(): void {
  resizePhotoHeight()
  handlers?.windowResized()
}

/**
 * Nastaví rozměry mapy podle okna (fitMapToWindow).
 * @param heightOfDialog výška panelu v % (jen mobil)
 * @param mapWidth šířka mapy (desktop: "67%" při otevřeném panelu)
 */
export function fitMapToWindow(heightOfDialog: number | null = null, mapWidth: string | null = null): void {
  // Výška okna bez horního pruhu stránky (body { border-top } v site.css). site.ts s ním nepočítal,
  // dokument byl o pruh vyšší než okno a na mobilu šel posunout – lišta tlačítek pak nebyla u spodního
  // okraje a pod ní (nebo nad spodním panelem) prosvítala mapa.
  const bodyBorder = parseFloat(getComputedStyle(document.body).borderTopWidth) || 0
  const pageHeight = window.innerHeight - bodyBorder
  const mobile = isMobileView()
  const controlButtons = document.getElementById('controlButtonsWrapper')

  // výška spodního panelu – panel má height: X % celého okna (ne okna bez pruhu)
  const tempHeight = mobile
    ? (window.innerHeight * (heightOfDialog != null ? heightOfDialog : 0)) / 100
    : pageHeight - (controlButtons != null ? controlButtons.offsetTop : 0)

  const mapElement = document.getElementById('map')
  const pageElement = document.getElementsByClassName('page')[0] as HTMLElement | undefined
  if (!mapElement) return

  mapElement.style.width = mapWidth == null ? '100%' : mapWidth

  // Mobil: horní lišta (44 px) je absolutně od samého vrchu okna a překrývá pruh body { border-top },
  // mapa ale začíná až pod pruhem. Odsazení se proto zmenší o pruh, aby mapa navazovala přímo na lištu
  // (site.ts odsazoval o celých 44 px a mezi lištou a mapou zůstal béžový pruh). Mapa končí na spodním
  // okraji okna, resp. u horního okraje spodního panelu.
  const mobileTopBar = 44
  const mapHeight = !mobile ? pageHeight - 1 : window.innerHeight - mobileTopBar - tempHeight
  mapElement.style.height = mapHeight + 'px'

  const marginTop = mobile ? Math.max(0, mobileTopBar - bodyBorder) : 0
  mapElement.style.marginTop = mobile ? marginTop + 'px' : ''
  // odsazení mapy se přenese (margin collapsing) i na .page a posune ji dolů – výška .page ho proto odečítá,
  // jinak by dokument byl vyšší než okno
  if (pageElement) pageElement.style.height = !mobile ? pageHeight - 1 + 'px' : pageHeight - marginTop + 'px'

  map?.invalidateSize()
}

// ------------------------------------------------------------------
// URL
// ------------------------------------------------------------------

/** Parametr "bounds" z URL: "jihZápadLat,jihZápadLng,severVýchodLat,severVýchodLng" (zapisuje setUrlByMapInfo). */
function getBoundsFromUrl(): L.LatLngBounds | null {
  const values = getUrlParam('bounds')?.split(',').map(Number)
  if (!values || values.length !== 4 || values.some((v) => !Number.isFinite(v))) return null
  return L.latLngBounds([values[0], values[1]], [values[2], values[3]])
}

/**
 * Nastaví mapu podle parametrů URL (setMapWithInfoFromUrl + getMapInfoFromUrl):
 * 1) výřez "bounds" (+ "zoom"), 2) vybrané místo "x1/y1" (+ "zoom").
 * Pozn.: v site.ts se "bounds" kvůli chybě nikdy nepoužil (sdílený odkaz otevřel výchozí pohled) – opraveno.
 */
function setMapWithInfoFromUrl(): boolean {
  if (!map) return false
  const zoomParam = getUrlParam('zoom')
  const zoom = Number(zoomParam ?? '13')

  const bounds = getBoundsFromUrl()
  if (bounds) {
    if (zoomParam != null && Number.isFinite(zoom)) map.setView(bounds.getCenter(), zoom)
    else map.fitBounds(bounds)
    return true
  }

  const x1 = getUrlParam('x1')
  const y1 = getUrlParam('y1')
  if (x1 != null && y1 != null) {
    map.setView([Number(y1), Number(x1)], zoom)
    return true
  }
  return false
}

/** Zapíše výřez a zoom do URL (setUrlByMapInfo) */
function setUrlByMapInfo(): void {
  if (!map) return
  const bounds = map.getBounds()
  const sw = bounds.getSouthWest()
  const ne = bounds.getNorthEast()
  setUrlParam('bounds', `${sw.lat},${sw.lng},${ne.lat},${ne.lng}`)
  setUrlParam('zoom', map.getZoom())
}

/** Podkladová (tile) nebo WMS vrstva (convertMapSettingsObjectToMapLayer) */
function convertMapSettingsObjectToMapLayer(layer: LayerForLeaflet): L.TileLayer | null {
  if (layer.type === LayerType.Base) {
    return new L.TileLayer(layer.url ?? '', { maxZoom: 18, attribution: layer.attribution ?? undefined })
  }
  if (layer.type === LayerType.WMS) {
    return L.tileLayer.wms(layer.url ?? '', {
      tileSize: 512,
      layers: layer.layersParameter ?? '',
      className: 'customWmsLayer',
    })
  }
  return null
}

// ------------------------------------------------------------------
// VOLÁNÍ DO APLIKACE (dříve Blazor)
// ------------------------------------------------------------------

/** callBlazor_RefreshObjectsOnMap */
export function callRefreshObjectsOnMap(): void {
  if (!map || !handlers) return
  // Pozn.: skupina AdditionalObjects je typu Polygons, takže tahle podmínka je vždy splněná a po posunu
  // mapy se polygony nenačítají znovu (načtou se jednou při inicializaci). Chování zachováno ze site.ts.
  const mapHasPolygonsYet = groups.some((a) => a.options.type === LayerType.Polygons)
  const bounds = getMapBounds()
  if (!bounds) return
  void handlers.refreshObjectsOnMap(!mapHasPolygonsYet, map.getZoom(), bounds)
}

/** callBlazor_ShowPlaceInfo – klik na marker nebo polygon */
function onObjectClick(event: L.LeafletEvent): void {
  if (!map || !handlers) return
  removeAdditionalObjects()
  unselectAllSelectedPoints()

  const target = event.target as L.Marker | L.Polygon
  let point: L.LatLngExpression
  if (target instanceof L.Marker) {
    const icon = target.getElement()
    if (icon) icon.className = icon.className.replace('map-point-selected', 'map-point').replace('map-point', 'map-point-selected')
    point = target.getLatLng()
  } else {
    target.setStyle({ fillColor: polygonColorSelected })
    point = [lat, lng]
  }

  const bbox = convertObjectPositionToBBoxParameter(point)
  void handlers.showPlaceInfo(map.getZoom(), bbox, coorx)
}

/** openStoryMapTimelineLabel – klik na hlavní bod příběhu */
function onStoryMapPointClick(event: L.LeafletEvent): void {
  const marker = event.target as AppMarker
  void handlers?.openStoryMapTimelineLabel(marker.options.stopId ?? 0, marker.options.narrativeMapId ?? null)
}

// ------------------------------------------------------------------
// OBJEKTY
// ------------------------------------------------------------------

/** Překreslí objekty, polygony a heatmapu (refreshObjectsOnMap). Nepřístupná místa (polygony) zůstávají. */
export function refreshObjectsOnMap(objects: MapObjectForLeaflet[]): void {
  if (!map || groups.length === 0) return

  const objectsGroup = findGroup('Objects_group')
  const polygonsGroups = groups.filter((a) => a.options.type === LayerType.Polygons || a.options.type === LayerType.WFS)
  if (!objectsGroup) return

  objectsGroup.clearLayers()

  for (const object of objects) {
    if (object.mapPolygon != null) {
      const polygon = getPolygon(object, JSON.parse(object.mapPolygon) as GeoJsonPolygon)
      const layerName = object.layerName?.toLocaleLowerCase()
      if (layerName != null && layerName !== 'statistics') {
        const target = polygonsGroups.find((a) => a.options.name?.toLocaleLowerCase() === layerName)
        if (target) polygon.addTo(target)
      } else {
        polygon.addTo(objectsGroup)
      }
    } else if (object.mapPoint != null && !object.heatmap) {
      const marker = getPoint(object, JSON.parse(object.mapPoint) as GeoJsonPoint, 'map-point')
      marker.options.guid = object.guid
      marker.addTo(objectsGroup)
    }
  }
  for (const group of polygonsGroups) group.bringToFront()

  if (heatmapLayer) {
    map.removeLayer(heatmapLayer)
    heatmapLayer = null
  }
  const heatmapIndex = objects.find((a) => a.heatmap)?.placeType ?? null
  if (heatmapIndex != null) {
    seedHeatmapData(objects, heatmapIndex)
    const entry = heatmapData?.find((a) => a.id === heatmapIndex)
    if (entry) showHeatmap(entry.heatmapdata)
  }

  setZIndexForGroups()
}

function setZIndexForGroups(): void {
  for (const layer of mapSettings?.layers ?? []) {
    const group = groups.find((a) => a.options.name === layer.name)
    if (group && layer.zIndex != null) group.setZIndex(layer.zIndex)
  }
}

/** Data heatmapy se načtou jednou pro každý typ místa a pak se jen znovu zobrazují (seedHeatmapData). */
function seedHeatmapData(objects: MapObjectForLeaflet[], heatmapIndex: string): void {
  if (heatmapData?.some((a) => a.id === heatmapIndex)) return
  const entry = { id: heatmapIndex, heatmapdata: { max: 100, data: [] } as HeatmapData }
  heatmapData = [...(heatmapData ?? []), entry]
  for (const object of objects) {
    if (object.mapPoint != null && object.heatmap) {
      const point = JSON.parse(object.mapPoint) as GeoJsonPoint
      entry.heatmapdata.data.push({ lat: point.coordinates[1], lng: point.coordinates[0], count: object.citizens ?? 0 })
    }
  }
}

function showHeatmap(data: HeatmapData): void {
  if (!map || data.data.length === 0) return
  heatmapLayer = new HeatmapOverlay({
    radius: 20,
    maxOpacity: 1,
    scaleRadius: false,
    useLocalExtrema: true,
    latField: 'lat',
    lngField: 'lng',
    valueField: 'count',
  })
  heatmapLayer.setData(data)
  heatmapLayer.addTo(map)
}

/** Přidá objekty do skupiny (addObjectsFromJsonString) – místa příběhových map, výsledky hledání. */
export function addObjects(objects: MapObjectForLeaflet[], group: string): void {
  const groupName = group + '_group'
  const objectsGroup = findGroup(groupName)
  if (!objectsGroup) return
  objectsGroup.clearLayers()

  for (const object of objects) {
    if (!object.mapPoint) continue
    const location = JSON.parse(object.mapPoint) as GeoJsonPoint | GeoJsonPolygon | null
    if (location == null) continue

    const layer =
      location.type === 'Point'
        ? getPoint(
            object,
            location as GeoJsonPoint,
            'z-index-999',
            object.placeType === NarrativeMapStopPlaceType.Main ? onStoryMapPointClick : null,
          )
        : getPolygon(object, location as GeoJsonPolygon)
    layer.addTo(objectsGroup)
  }
  toggleLayerGroup(group, false)
  toggleLayerGroup(group, true)
  fitMapToGroup(groupName)
  drawCurveBetweenPoints(groupName)
}

function fitMapToGroup(groupName: string): void {
  const group = findGroup(groupName)
  if (!map || !group) return
  const latlngs: L.LatLng[] = []
  group.eachLayer((layer) => {
    if (layer instanceof L.Marker) latlngs.push(layer.getLatLng())
  })
  if (latlngs.length === 0) return
  map.fitBounds(L.latLngBounds(latlngs), { padding: [100, 100] })
}

/** Marker (getPoint) */
function getPoint(
  object: MapObjectForLeaflet,
  location: GeoJsonPoint,
  className?: string,
  clickFunction?: L.LeafletEventHandlerFn | null,
): AppMarker {
  const options: MarkerExtraOptions = {}
  if (object.htmlIcon != null) {
    options.stopId = object.stopId
    options.narrativeMapId = object.narrativeMapId
    options.type = object.placeType
    options.icon = new L.DivIcon({
      className,
      html: object.htmlIcon,
      iconAnchor: object.iconAnchor ?? undefined,
    })
  }

  const marker = new L.Marker([location.coordinates[1], location.coordinates[0]], options) as AppMarker
  marker.setZIndexOffset(object.priorityOnMap ?? 0)

  if (object.label != null) marker.bindTooltip(object.label, { sticky: true })
  if (object.clickable) marker.on('click', clickFunction ?? onObjectClick)
  marker.on('click', (e: L.LeafletMouseEvent) => openTooltipOnClick(marker, e))

  return marker
}

/** Polygon nebo multipolygon (getPolygon) */
function getPolygon(object: MapObjectForLeaflet, location: GeoJsonPolygon): L.Polygon {
  // převod souřadnic [lng, lat] → [lat, lng] stejně jako v site.ts (pro Polygon i MultiPolygon)
  const pointsArray: unknown[][] = []
  for (let j = 0; j < location.coordinates.length; j++) {
    pointsArray.push([])
    for (let m = 0; m < location.coordinates[j].length; m++) {
      const polygon = location.coordinates[j][m] as unknown[]
      const inner: number[][] = []
      for (let k = 0; k < polygon.length; k++) {
        const p = polygon[k] as number[]
        if (p[0] !== undefined && p[1] !== undefined) inner.push([p[1], p[0]])
        else inner.push([polygon[1] as number, polygon[0] as number])
      }
      if (inner.length > 0) pointsArray[j].push(inner)
    }
  }

  const options: PolygonExtraOptions =
    object.customPolygonClass != null
      ? {
          // statistiky čtvrtí
          className: object.customPolygonClass,
          color: object.customPolygonColor ?? undefined,
          fillOpacity: object.customPolygonFillOpacity ?? undefined,
          fillColor: object.customPolygonFillColor ?? undefined,
          smoothFactor: 0,
        }
      : {
          // nepřístupná místa a další polygony
          fillColor: object.customPolygonFillColor ?? polygonFillColorDefault,
          color: object.customPolygonColor ?? polygonColorDefault,
          weight: object.customPolygonWeight ?? 0.5,
          fillOpacity: object.customPolygonFillOpacity ?? 0.4,
          opacity: 1,
          smoothFactor: 0,
        }
  options.originalFillColor = options.fillColor

  const polygon = new L.Polygon(pointsArray as L.LatLngExpression[][][], options)

  if (object.clickable) polygon.on('click', onObjectClick)
  polygon.on('click', (e: L.LeafletMouseEvent) => openTooltipOnClick(polygon, e, e.latlng))

  if (object.label != null) {
    polygon.bindTooltip(object.label, { sticky: true, className: object.customTooltipClass ?? undefined })
  }
  return polygon
}

/** Odstraní objekty skupiny kromě modrého bodu polohy (removeObjects) */
function removeObjects(group: string): void {
  const objectsGroup = findGroup(group + '_group')
  if (!objectsGroup) return
  objectsGroup.eachLayer((item) => {
    if ((item.options as MarkerExtraOptions).type !== 'bluepoint') objectsGroup.removeLayer(item)
  })
}

export function removeAdditionalObjects(): void {
  removeObjects('AdditionalObjects')
  for (const group of groups.filter((a) => a.options.type === LayerType.Polygons)) {
    group.eachLayer((item) => {
      const polygon = item as L.Polygon
      const options = polygon.options as PolygonExtraOptions
      if (options.fillColor === polygonColorSelected) {
        polygon.setStyle({ fillColor: options.originalFillColor ?? polygonFillColorDefault })
      }
    })
  }
}

function elementClassName(element: Element): string | null {
  const className = (element as HTMLElement).className as unknown
  if (typeof className === 'string') return className
  if (className && typeof className === 'object' && 'baseVal' in className) return (className as SVGAnimatedString).baseVal
  return null
}

export function unselectAllSelectedPoints(): void {
  const objectsGroup = findGroup('Objects_group')
  if (!map || !objectsGroup) return
  objectsGroup.eachLayer((item) => {
    const marker = item as AppMarker
    const element = marker.getElement?.()
    if (!element) return
    const className = elementClassName(element)
    if (className && className.indexOf('map-point-selected') !== -1) {
      element.setAttribute('class', className.replace('map-point-selected', 'map-point'))
      element.style.zIndex = (
        map!.latLngToLayerPoint(marker.getLatLng()).round().y + (marker.options.zIndexOffset ?? 0)
      ).toString()
    }
  })
}

export function selectPointOnMap(guids: string[]): void {
  const objectsGroup = findGroup('Objects_group')
  if (!map || !objectsGroup) return
  objectsGroup.eachLayer((item) => {
    const marker = item as AppMarker
    const element = marker.getElement?.()
    if (element && marker.options.guid != null && guids.indexOf(marker.options.guid) > -1) {
      element.className = element.className.replace('map-point-selected', 'map-point').replace('map-point', 'map-point-selected')
      element.style.zIndex = (map!.latLngToLayerPoint(marker.getLatLng()).round().y * 2 + (marker.options.zIndexOffset ?? 0)).toString()
      marker.openTooltip()
    }
  })
}

// ------------------------------------------------------------------
// KŘIVKY MEZI BODY TRAJEKTORIE (příběhové mapy)
// ------------------------------------------------------------------

const CURVE_CFG = {
  minArrowDist: 15, // min. vzdálenost konce šipky od cílového bodu (px)
  maxArrowDist: 25, // max. vzdálenost konce šipky od cílového bodu (px)
  maxShortenRatio: 0.4, // max. zkrácení jako poměr celkové délky
  shortDistThreshold: 100, // práh pro přepnutí na vizuální oblouk (px)
  arcHeightMin: 10, // min. výška oblouku (px)
  arcHeightRatio: 0.15, // poměr výšky oblouku ke vzdálenosti
  curveSegments: 100, // počet segmentů křivky
  fallbackShortenRatio: 0.3, // max. zkrácení pro fallback
}

/** Vykreslí křivky se šipkami mezi body trajektorie ve skupině (drawCurveBettwenPoints). */
function drawCurveBetweenPoints(groupName: string, shortenEndPixels = 10): void {
  const group = findGroup(groupName)
  if (!group) return

  group.eachLayer((layer) => {
    if ((layer.options as { isTrajectoryLine?: boolean }).isTrajectoryLine) group.removeLayer(layer)
  })

  const markers: L.Marker[] = []
  group.eachLayer((layer) => {
    if (layer instanceof L.Marker && (layer as AppMarker).options.type === NarrativeMapStopPlaceType.Trajectory) {
      markers.push(layer)
    }
  })

  for (let i = 0; i < markers.length - 1; i++) {
    const start = markers[i].getLatLng()
    const end = markers[i + 1].getLatLng()
    // poslední šipka mířící dolů potřebuje větší mezeru (vysoký špendlík by ji překryl)
    const isLastSegment = i === markers.length - 2
    const isPointingSouth = end.lat < start.lat
    const shorten = isLastSegment && isPointingSouth ? shortenEndPixels * 3.5 : shortenEndPixels
    const curve = getCurve(start, end, shorten)
    curve?.addTo(group)
  }
}

function getVisualArcControlPoint(p1: L.Point, p2: L.Point, pixDist: number): number[] {
  const mid = p1.add(p2).divideBy(2)
  const vec = p2.subtract(p1)
  const perpX = -vec.y
  const perpY = vec.x
  const len = Math.sqrt(perpX * perpX + perpY * perpY)
  const arcHeight = Math.max(CURVE_CFG.arcHeightMin, pixDist * CURVE_CFG.arcHeightRatio)
  const ctrl = map!.containerPointToLatLng(L.point(mid.x + (perpX / len) * arcHeight, mid.y + (perpY / len) * arcHeight))
  return [ctrl.lat, ctrl.lng]
}

function getGeoCurveControlPoint(a: L.LatLng, b: L.LatLng): number[] {
  const distance = map!.distance(a, b)
  const minDistance = 50
  const maxDistance = 50000
  const normalized = Math.min(Math.max((distance - minDistance) / (maxDistance - minDistance), 0), 1)
  const curveFactor = Math.pow(normalized, 3)
  const maxDiff = Math.max(Math.abs(b.lat - a.lat), Math.abs(b.lng - a.lng))
  const offset = maxDiff * (0.02 + 0.06 * curveFactor)
  return [(a.lat + b.lat) / 2 + offset, (a.lng + b.lng) / 2]
}

function sampleQuadraticBezier(start: number[], control: number[], end: number[], segments: number): number[][] {
  const points: number[][] = []
  for (let t = 0; t <= 1; t += 1 / segments) {
    const inv = 1 - t
    const x = inv * inv * start[1] + 2 * inv * t * control[1] + t * t * end[1]
    const y = inv * inv * start[0] + 2 * inv * t * control[0] + t * t * end[0]
    points.push([y, x])
  }
  return points
}

function shortenCurveFromEndByPixels(points: number[][], targetPx: number): number[][] {
  if (points.length < 2 || targetPx <= 0) return points
  let cumulative = 0
  for (let i = points.length - 1; i > 0; i--) {
    const curr = map!.latLngToContainerPoint(L.latLng(points[i][0], points[i][1]))
    const prev = map!.latLngToContainerPoint(L.latLng(points[i - 1][0], points[i - 1][1]))
    const segDist = curr.distanceTo(prev)
    cumulative += segDist
    if (cumulative >= targetPx) {
      if (segDist > 0) {
        const ratio = (cumulative - targetPx) / segDist
        const newPt = map!.containerPointToLatLng(L.point(prev.x + (curr.x - prev.x) * ratio, prev.y + (curr.y - prev.y) * ratio))
        const result = points.slice(0, i)
        result.push([newPt.lat, newPt.lng])
        return result
      }
      return points.slice(0, i)
    }
  }
  // křivka je kratší než targetPx – zkrátit max. o 30 %
  if (cumulative > 0) {
    const ratio = Math.min(targetPx / cumulative, CURVE_CFG.fallbackShortenRatio)
    return points.slice(0, Math.max(2, Math.ceil(points.length * (1 - ratio))))
  }
  return points
}

/** Křivka se šipkou mezi dvěma body (getCurve) */
function getCurve(a: L.LatLng, b: L.LatLng, shortenEndPixels: number, color = '#771646'): L.Polyline | null {
  if (!map) return null
  const p1 = map.latLngToContainerPoint(a)
  const p2 = map.latLngToContainerPoint(b)
  const pixDist = p1.distanceTo(p2)
  if (pixDist < 1) return null

  let shorten = Math.max(CURVE_CFG.minArrowDist, Math.min(CURVE_CFG.maxArrowDist, shortenEndPixels))
  if (shorten > pixDist * CURVE_CFG.maxShortenRatio) shorten = pixDist * CURVE_CFG.maxShortenRatio

  const control = pixDist < CURVE_CFG.shortDistThreshold ? getVisualArcControlPoint(p1, p2, pixDist) : getGeoCurveControlPoint(a, b)
  let points = sampleQuadraticBezier([a.lat, a.lng], control, [b.lat, b.lng], CURVE_CFG.curveSegments)
  points = shortenCurveFromEndByPixels(points, shorten)
  if (points.length < 2) return null

  return L.polyline(points as L.LatLngExpression[], {
    color,
    weight: 1,
    dashArray: '5, 5',
    dashOffset: '0',
    isTrajectoryLine: true,
  } as L.PolylineOptions).arrowheads({ frequency: 'endonly', fill: true, size: '15px', color })
}

// ------------------------------------------------------------------
// OSOBNÍ TRAJEKTORIE (narativní mapy typu 6)
// ------------------------------------------------------------------

let trajectoryStops: TrajectoryStopForLeaflet[] | null = null
let trajectoryMaxZoom = 18

/** Špendlík v barvě zastavení s pořadovým číslem */
function trajectoryPinHtml(color: string, order: number): string {
  return (
    '<svg xmlns="http://www.w3.org/2000/svg" width="26" height="36" viewBox="0 0 26 36">' +
    `<path d="M13 35C13 35 24.5 23 24.5 13.5A11.5 11.5 0 0 0 1.5 13.5C1.5 23 13 35 13 35Z" fill="${color}" stroke="#fff" stroke-width="1.5"/>` +
    `<text x="13" y="17.5" text-anchor="middle" font-family="Barlow, sans-serif" font-size="11" font-weight="700" fill="#fff">${order}</text>` +
    '</svg>'
  )
}

/** Zobrazí trajektorii ve vrstvě Narration a přizpůsobí výřez mapy všem jejím bodům. */
export function showTrajectory(stops: TrajectoryStopForLeaflet[], maxZoom: number): void {
  const group = findGroup(LayerType.Narration + '_group')
  if (!map || !group) return
  group.clearLayers()
  trajectoryStops = stops
  trajectoryMaxZoom = maxZoom || 18

  stops.forEach((stop, i) => {
    const marker = new L.Marker([stop.lat, stop.lng], {
      icon: L.divIcon({
        className: 'trajectory-stop',
        html: trajectoryPinHtml(stop.color, i + 1),
        iconSize: [26, 36],
        iconAnchor: [13, 36],
        tooltipAnchor: [0, -34],
      }),
      zIndexOffset: 1000 + i,
    })
    marker.bindTooltip(stop.tooltip, { direction: 'top', className: 'trajectory-tooltip' })
    // klik: vycentrovat a přiblížit na maximum (na mobilu se zároveň ukáže popisek)
    marker.on('click', (e: L.LeafletMouseEvent) => {
      openTooltipOnClick(marker, e)
      map?.setView(marker.getLatLng(), trajectoryMaxZoom)
    })
    marker.addTo(group)
  })

  if (!map.hasLayer(group)) group.addTo(map)
  // spojnice závisí na měřítku (oblouky v pixelech) – po každé změně zoomu se překreslí
  map.off('zoomend', drawTrajectoryLines)
  map.on('zoomend', drawTrajectoryLines)

  if (stops.length > 0) {
    const bounds = L.latLngBounds(stops.map((a) => [a.lat, a.lng] as L.LatLngTuple))
    map.fitBounds(bounds, { padding: [50, 50], maxZoom: Math.min(trajectoryMaxZoom, 16) })
  }
  drawTrajectoryLines()
}

/** Odstraní trajektorii z mapy. */
export function hideTrajectory(): void {
  trajectoryStops = null
  map?.off('zoomend', drawTrajectoryLines)
  const group = findGroup(LayerType.Narration + '_group')
  if (!group) return
  group.clearLayers()
  if (map?.hasLayer(group)) map.removeLayer(group)
}

/** Spojnice mezi po sobě jdoucími zastaveními – barva podle cílového zastavení. */
function drawTrajectoryLines(): void {
  const group = findGroup(LayerType.Narration + '_group')
  if (!map || !group || !trajectoryStops) return
  group.eachLayer((layer) => {
    if ((layer.options as { isTrajectoryLine?: boolean }).isTrajectoryLine) group.removeLayer(layer)
  })
  for (let i = 0; i < trajectoryStops.length - 1; i++) {
    const start = L.latLng(trajectoryStops[i].lat, trajectoryStops[i].lng)
    const end = L.latLng(trajectoryStops[i + 1].lat, trajectoryStops[i + 1].lng)
    // šipka mířící dolů by skončila pod špendlíkem cíle – větší mezera
    const shorten = end.lat < start.lat ? 35 : 10
    getCurve(start, end, shorten, trajectoryStops[i + 1].color)?.addTo(group)
  }
}

/** Aktuální střed a zoom mapy (pro návrat po zavření trajektorie) */
export function getView(): { center: Coordinate; zoom: number } | null {
  if (!map) return null
  const c = map.getCenter()
  return { center: { x: c.lng, y: c.lat }, zoom: map.getZoom() }
}

export function setView(center: Coordinate, zoom: number): void {
  map?.setView([center.y, center.x], zoom)
}

// ------------------------------------------------------------------
// VRSTVY
// ------------------------------------------------------------------

export function hideLayersForNormalMap(): void {
  if (!map) return
  for (const group of groups) {
    if (group.options.type === LayerType.Base || group.options.type === LayerType.WMS) continue
    map.removeLayer(group)
  }
}

export function showLayersForNormalMap(): void {
  if (!map) return
  hideLayersForNormalMap()
  for (const group of groups) {
    if (group.options.type === LayerType.Base || group.options.type === LayerType.Narration) continue
    if (group.options.selected) group.addTo(map)
  }
}

export function toggleLayerGroup(name: string, selected: boolean): void {
  const group = groups.find((a) => a.options.name === name)
  if (!map || !group) return
  group.options.selected = selected
  map.removeLayer(group)
  if (selected) group.addTo(map)
}

// ------------------------------------------------------------------
// POMOCNÉ
// ------------------------------------------------------------------

/** Malý čtverec (5 px) kolem bodu jako dotaz na místa v okolí kliknutí (convertObjectPositionToBBoxParameter) */
function convertObjectPositionToBBoxParameter(point: L.LatLngExpression): Coordinate[] {
  const pos = map!.latLngToContainerPoint(point)
  const size = 5
  const minx = pos.x < size / 2 ? 0 : pos.x - size / 2
  const miny = pos.y < size / 2 ? 0 : pos.y + size / 2
  const maxx = pos.x < size / 2 ? 0 : pos.x + size / 2
  const maxy = pos.y < size / 2 ? 0 : pos.y - size / 2
  const sw = map!.containerPointToLatLng([minx, maxy])
  const ne = map!.containerPointToLatLng([maxx, miny])
  return [
    { x: sw.lng, y: sw.lat },
    { x: ne.lng, y: ne.lat },
  ]
}

export function getMapBounds(): [Coordinate, Coordinate] | null {
  if (!map) return null
  const bounds = map.getBounds()
  return [
    { x: bounds.getSouthWest().lng, y: bounds.getSouthWest().lat },
    { x: bounds.getNorthEast().lng, y: bounds.getNorthEast().lat },
  ]
}

export function getZoom(): number {
  return map?.getZoom() ?? 0
}

export function toggleScaleVisibility(visible: boolean): void {
  const scale = document.getElementsByClassName('leaflet-control-scale')[0] as HTMLElement | undefined
  if (scale) scale.style.visibility = visible ? '' : 'hidden'
}

export function goToLocation(point: Coordinate, zoom: number): void {
  map?.flyTo([point.y, point.x], zoom)
}

// ------------------------------------------------------------------
// SLEDOVÁNÍ POLOHY
// ------------------------------------------------------------------

export function turnOnLocationTracking(): void {
  if (!navigator.geolocation) {
    console.log("Your browser doesn't support geolocation feature!")
    return
  }
  if (watchId !== null) navigator.geolocation.clearWatch(watchId)
  watchId = navigator.geolocation.watchPosition(showMyLocation, handleLocationError, {
    enableHighAccuracy: true,
    timeout: 20000, // delší timeout kvůli iOS
    maximumAge: 0,
  })
}

function handleLocationError(error: GeolocationPositionError): void {
  console.warn(`ERROR(${error.code}): ${error.message}`)
  switch (error.code) {
    case error.PERMISSION_DENIED:
      alert('User denied the request for Geolocation.')
      break
    case error.POSITION_UNAVAILABLE:
      alert('Location information is unavailable.')
      break
    case error.TIMEOUT:
      alert('The request to get user location timed out.')
      break
    default:
      alert('An unknown error occurred while retrieving location.')
  }
}

function hideMyLocation(): void {
  if (watchId !== null) {
    navigator.geolocation.clearWatch(watchId)
    watchId = null
  }
  applicationIsTrackingLocation = false
  removeBluepoint()
}

export function goToMyLocation(): void {
  if (!map || actualLocation == null) return
  map.setView([actualLocation.coords.latitude, actualLocation.coords.longitude], 15)
}

function showMyLocation(position: GeolocationPosition): void {
  actualLocation = position
  removeBluepoint()
  addBluepoint([position.coords.latitude, position.coords.longitude])
  if (!applicationIsTrackingLocation) {
    goToMyLocation()
    applicationIsTrackingLocation = true
  }
}

function addBluepoint(point: L.LatLngExpression): void {
  const group = findGroup('AdditionalObjects_group')
  if (!group || !bluepointIcon) return
  new L.Marker(point, { icon: bluepointIcon, type: 'bluepoint' } as MarkerExtraOptions).addTo(group)
}

function removeBluepoint(): void {
  const group = findGroup('AdditionalObjects_group')
  if (!group) return
  group.eachLayer((item) => {
    if ((item.options as MarkerExtraOptions).type === 'bluepoint') group.removeLayer(item)
  })
}

// ------------------------------------------------------------------
// DIALOG
// ------------------------------------------------------------------

/**
 * Panel přes celou obrazovku (setDialogFullScreen). Rozměry panelu řídí SideDialog podle store.isDialogFullScreen,
 * tady se jen přizpůsobí mapa – stejné hodnoty, jaké site.ts předával do fitMapToWindow.
 */
export function setDialogFullScreen(value: boolean): void {
  if (value) {
    isFullscreen = true
    fitMapToWindow(window.innerHeight - 44)
  } else if (isFullscreen) {
    isFullscreen = false
    fitMapToWindow(mobileDialogHeight)
  }
}

// ------------------------------------------------------------------
// IMPLEMENTACE MapController
// ------------------------------------------------------------------

export const mapApi: MapController = {
  initMap,
  destroyMap,
  refreshObjectsOnMap,
  addObjects,
  removeAdditionalObjects,
  selectPointOnMap,
  unselectAllSelectedPoints,
  showLayersForNormalMap,
  hideLayersForNormalMap,
  toggleLayerGroup,
  resetMapViewToInitialState,
  toggleScaleVisibility,
  fitMapToWindow,
  goToLocation,
  zoomIn: () => map?.zoomIn(),
  zoomOut: () => map?.zoomOut(),
  getZoom,
  getMapBounds,
  turnOnLocationTracking,
  goToMyLocation,
  setDialogFullScreen,
  requestRefresh: callRefreshObjectsOnMap,
  setUrlFromSelectedPlace: writeSelectedPlaceToUrl,
  resizePhotoHeight,
  closeGallery,
  showTrajectory,
  hideTrajectory,
  getView,
  setView,
}
