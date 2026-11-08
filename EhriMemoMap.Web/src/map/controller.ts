// Rozhraní mezi stavem aplikace (stores/map.ts) a Leaflet mapou (map/mapApi.ts).
//
// V Blazoru volal MapService JavaScript přes IJSRuntime ("mapAPI.xxx") a JS volal zpět .NET přes
// DotNetObjectReference. Tady store volá mapu přes MapController a mapa volá store přes MapEventHandlers.
// Implementaci MapController registruje mapApi.ts (setMapController) při inicializaci mapy; dokud mapa
// neexistuje, jsou volání no-op – store tak nezávisí přímo na Leafletu (a jde použít bez mapy, např. v testech).
import type { Coordinate, MapObjectForLeaflet, MapSettingsForLeaflet } from '../models/map'

export interface MapController {
  /** mapAPI.initMap */
  initMap(settings: MapSettingsForLeaflet): void
  /** mapAPI.destroyMap */
  destroyMap(): void
  /** mapAPI.refreshObjectsOnMap – překreslí objekty, polygony a heatmapu */
  refreshObjectsOnMap(objects: MapObjectForLeaflet[]): void
  /** mapAPI.addObjectsFromJsonString – přidá objekty do skupiny (Narration, AdditionalObjects) */
  addObjects(objects: MapObjectForLeaflet[], group: string): void
  removeAdditionalObjects(): void
  selectPointOnMap(guids: string[]): void
  unselectAllSelectedPoints(): void
  showLayersForNormalMap(): void
  hideLayersForNormalMap(): void
  toggleLayerGroup(name: string, selected: boolean): void
  resetMapViewToInitialState(): void
  toggleScaleVisibility(visible: boolean): void
  /** mapAPI.fitMapToWindow(heightOfDialog, mapWidth) */
  fitMapToWindow(heightOfDialog?: number | null, mapWidth?: string | null): void
  /** Vycentruje mapu na bod (mapAPI.goToLocation) */
  goToLocation(point: Coordinate, zoom: number): void
  zoomIn(): void
  zoomOut(): void
  getZoom(): number
  /** [jihozápadní, severovýchodní] roh zobrazené oblasti */
  getMapBounds(): [Coordinate, Coordinate] | null
  turnOnLocationTracking(): void
  goToMyLocation(): void
  setDialogFullScreen(value: boolean): void
  /** Požádá mapu o nové načtení objektů (mapAPI.callBlazor_RefreshObjectsOnMap) */
  requestRefresh(): void
  /** Zapíše vybrané místo do URL (mapAPI.setUrlFromSelectedPlace) */
  setUrlFromSelectedPlace(points: Coordinate[]): void
  resizePhotoHeight(): void
  closeGallery(): void
}

/** Volání z mapy do stavu aplikace (náhrada [JSInvokable] metod v Map.razor). */
export interface MapEventHandlers {
  refreshObjectsOnMap(withPolygons: boolean, zoom: number, bounds: [Coordinate, Coordinate]): void | Promise<void>
  showPlaceInfo(zoom: number, coordinates: Coordinate[], mouseClickPointX: number): void | Promise<void>
  setMobileView(value: boolean): void
  openStoryMapTimelineLabel(stopId: number, narrativeMapId: number | null): void | Promise<void>
}

const noop = () => {}

const nullController: MapController = {
  initMap: noop,
  destroyMap: noop,
  refreshObjectsOnMap: noop,
  addObjects: noop,
  removeAdditionalObjects: noop,
  selectPointOnMap: noop,
  unselectAllSelectedPoints: noop,
  showLayersForNormalMap: noop,
  hideLayersForNormalMap: noop,
  toggleLayerGroup: noop,
  resetMapViewToInitialState: noop,
  toggleScaleVisibility: noop,
  fitMapToWindow: noop,
  goToLocation: noop,
  zoomIn: noop,
  zoomOut: noop,
  getZoom: () => 0,
  getMapBounds: () => null,
  turnOnLocationTracking: noop,
  goToMyLocation: noop,
  setDialogFullScreen: noop,
  requestRefresh: noop,
  setUrlFromSelectedPlace: noop,
  resizePhotoHeight: noop,
  closeGallery: noop,
}

let controller: MapController = nullController

export function setMapController(value: MapController | null): void {
  controller = value ?? nullController
}

/** Aktuální ovladač mapy (nikdy null – bez mapy vrací no-op implementaci). */
export function mapController(): MapController {
  return controller
}
