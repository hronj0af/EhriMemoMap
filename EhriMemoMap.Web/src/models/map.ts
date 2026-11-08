// Modely stavu mapy (EhriMemoMap.Shared/MapStateModels) a modely pro Leaflet (EhriMemoMap.Client/Models).
import type { DialogType, FontColor, LayerType, MapType, PlaceType } from './enums'
import type { NarrativeMap, VictimLongInfoModel } from './api'

/** Souřadnice bodu (NTS Coordinate v Blazor klientovi): x = zeměpisná délka, y = šířka. */
export interface Coordinate {
  x: number
  y: number
}

export interface InitialVariables {
  city: string
  allCities: string[] | null
  storyMaps: boolean | null
  zoom: number
  zoomMobile: number
  minZoom: number
  maxZoom: number
  lat: number
  lng: number
  latMobile: number
  lngMobile: number
  minBounds: Coordinate | null
  maxBounds: Coordinate | null
  heightOfDialog: number
}

/** Vrstva z mapsettings.{city}.json (po normalizaci – enumy v PascalCase, čísla jako number). */
export interface LayerModel {
  name: string | null
  url: string | null
  type: LayerType | null
  placeType: PlaceType | null
  attribution: string | null
  opacities: string | null
  zIndex: number | null
  cssClass: string | null
  mapParameter: string | null
  layersParameter: string | null
  selected: boolean
  priorityOnMap: number
  order: number
  quickAccess: boolean
  backgroundColor: string | null
  fontColor: FontColor | null
  customPolygonColor: string | null
  customPolygonFillColor: string | null
  customPolygonFillOpacity: number | null
  customPolygonWeight: number | null
  labelCode: string | null
  minZoom: number | null
  maxZoom: number | null
}

export interface TimelinePointModel {
  name: string | null
  title: string | null
  selected: boolean
  /** yyyy-MM-dd (případně ISO datum/čas) */
  from: string | null
  to: string | null
  additionalLayers: LayerModel[] | null
}

export interface MapModel {
  initialVariables: InitialVariables | null
  layers: LayerModel[] | null
  timeline: TimelinePointModel[] | null
}

/** Objekt na mapě ve tvaru, který zpracovává Leaflet kód (MapObjectForLeafletModel). */
export interface MapObjectForLeaflet {
  clickable: boolean
  placeType: string | null
  layerName: string | null
  citizens: number | null
  citizensTotal: number | null
  id: string | null
  guid: string | null
  label: string | null
  customTooltipClass: string | null
  customPolygonClass: string | null
  customPolygonColor: string | null
  customPolygonFillColor: string | null
  customPolygonFillOpacity: number | null
  customPolygonWeight: number | null
  /** GeoJSON bodu jako řetězec */
  mapPoint: string | null
  /** GeoJSON polygonu jako řetězec */
  mapPolygon: string | null
  htmlIcon: string | null
  iconUrl: string | null
  iconAnchor: [number, number] | null
  iconSize: [number, number] | null
  heatmap: boolean | null
  stopId: number | null
  narrativeMapId: number | null
  priorityOnMap: number | null
}

/** Vrstva předávaná Leafletu při inicializaci (LayerForLeafletModel). */
export interface LayerForLeaflet {
  name: string | null
  url?: string | null
  type: string | null
  selected: boolean | null
  attribution?: string | null
  mapParameter?: string | null
  layersParameter?: string | null
  zIndex: number | null
  cssClass?: string | null
}

export interface MapSettingsForLeaflet {
  initialVariables: InitialVariables | null
  layers: LayerForLeaflet[]
}

export interface DialogParameters {
  id?: number | null
  places?: MapObjectForLeaflet[] | null
}

export interface DialogParametersHistoryItem {
  dialogType: DialogType
  mapType: MapType
  victimInfo: VictimLongInfoModel | null
  narrativeMap: NarrativeMap | null
  parameters: DialogParameters
}

/** Nastavení postranního panelu (Radzen SideDialogOptions z MapService.GetDialogOptions). */
export interface SideDialogOptions {
  position: 'right' | 'bottom'
  cssClass: string
  style: string
  height: string
  width: string
}
