// Načtení a normalizace mapsettings.{city}.json.
// V C# se soubor deserializoval Newtonsoftem do MapService/MapModel, který tiše převáděl typy:
// enumy case-insensitive ("polygons" → LayerType.Polygons), čísla v řetězcích ("zIndex": "50" → 50),
// PointModel {"X","Y"}. Tady se totéž dělá explicitně, aby zbytek aplikace pracoval s čistými typy.
import { parseLenientJson } from '../utils/json'
import { FontColor, LayerType, PlaceType, parseEnum } from './enums'
import type { Coordinate, InitialVariables, LayerModel, MapModel, TimelinePointModel } from './map'

type Raw = Record<string, unknown>

function asRaw(value: unknown): Raw | null {
  return value !== null && typeof value === 'object' && !Array.isArray(value) ? (value as Raw) : null
}

/** Hodnota vlastnosti bez ohledu na velikost písmen klíče (Newtonsoft je case-insensitive). */
function prop(obj: Raw | null, name: string): unknown {
  if (!obj) return undefined
  if (name in obj) return obj[name]
  const lower = name.toLowerCase()
  const key = Object.keys(obj).find((k) => k.toLowerCase() === lower)
  return key !== undefined ? obj[key] : undefined
}

function str(value: unknown): string | null {
  if (value === null || value === undefined) return null
  return String(value)
}

function num(value: unknown): number | null {
  if (value === null || value === undefined || value === '') return null
  const n = typeof value === 'number' ? value : Number(value)
  return Number.isFinite(n) ? n : null
}

function bool(value: unknown): boolean {
  return value === true || (typeof value === 'string' && value.toLowerCase() === 'true')
}

function coordinate(value: unknown): Coordinate | null {
  const raw = asRaw(value)
  if (!raw) return null
  const x = num(prop(raw, 'x'))
  const y = num(prop(raw, 'y'))
  return x === null || y === null ? null : { x, y }
}

export function normalizeLayer(value: unknown): LayerModel {
  const r = asRaw(value)
  return {
    name: str(prop(r, 'name')),
    url: str(prop(r, 'url')),
    type: parseEnum(LayerType, prop(r, 'type')),
    placeType: parseEnum(PlaceType, prop(r, 'placeType')),
    attribution: str(prop(r, 'attribution')),
    opacities: str(prop(r, 'opacities')),
    zIndex: num(prop(r, 'zIndex')),
    cssClass: str(prop(r, 'cssClass')),
    mapParameter: str(prop(r, 'mapParameter')),
    layersParameter: str(prop(r, 'layersParameter')),
    selected: bool(prop(r, 'selected')),
    priorityOnMap: num(prop(r, 'priorityOnMap')) ?? 0,
    order: num(prop(r, 'order')) ?? 0,
    quickAccess: bool(prop(r, 'quickAccess')),
    backgroundColor: str(prop(r, 'backgroundColor')),
    fontColor: parseEnum(FontColor, prop(r, 'fontColor')),
    customPolygonColor: str(prop(r, 'customPolygonColor')),
    customPolygonFillColor: str(prop(r, 'customPolygonFillColor')),
    customPolygonFillOpacity: num(prop(r, 'customPolygonFillOpacity')),
    customPolygonWeight: num(prop(r, 'customPolygonWeight')),
    labelCode: str(prop(r, 'labelCode')),
    minZoom: num(prop(r, 'minZoom')),
    maxZoom: num(prop(r, 'maxZoom')),
  }
}

function normalizeTimelinePoint(value: unknown): TimelinePointModel {
  const r = asRaw(value)
  const additional = prop(r, 'additionalLayers')
  return {
    name: str(prop(r, 'name')),
    title: str(prop(r, 'title')),
    selected: bool(prop(r, 'selected')),
    from: str(prop(r, 'from')),
    to: str(prop(r, 'to')),
    additionalLayers: Array.isArray(additional) ? additional.map(normalizeLayer) : null,
  }
}

function normalizeInitialVariables(value: unknown): InitialVariables | null {
  const r = asRaw(value)
  if (!r) return null
  const allCities = prop(r, 'allCities')
  const storyMaps = prop(r, 'storyMaps')
  return {
    city: str(prop(r, 'city')) ?? '',
    allCities: Array.isArray(allCities) ? allCities.map(String) : null,
    storyMaps: storyMaps === undefined || storyMaps === null ? null : bool(storyMaps),
    personalTrajectories: bool(prop(r, 'personalTrajectories') ?? false),
    homeTown: str(prop(r, 'homeTown')),
    zoom: num(prop(r, 'zoom')) ?? 0,
    zoomMobile: num(prop(r, 'zoomMobile')) ?? 0,
    minZoom: num(prop(r, 'minZoom')) ?? 0,
    maxZoom: num(prop(r, 'maxZoom')) ?? 0,
    lat: num(prop(r, 'lat')) ?? 0,
    lng: num(prop(r, 'lng')) ?? 0,
    latMobile: num(prop(r, 'latMobile')) ?? 0,
    lngMobile: num(prop(r, 'lngMobile')) ?? 0,
    minBounds: coordinate(prop(r, 'minBounds')),
    maxBounds: coordinate(prop(r, 'maxBounds')),
    heightOfDialog: num(prop(r, 'heightOfDialog')) ?? 0,
  }
}

/** Normalizuje obsah mapsettings.{city}.json (kořen má vlastnost "map"). */
export function normalizeMapSettings(json: unknown): MapModel | null {
  const map = asRaw(prop(asRaw(json), 'map'))
  if (!map) return null
  const layers = prop(map, 'layers')
  const timeline = prop(map, 'timeline')
  return {
    initialVariables: normalizeInitialVariables(prop(map, 'initialVariables')),
    layers: Array.isArray(layers) ? layers.map(normalizeLayer) : null,
    timeline: Array.isArray(timeline) ? timeline.map(normalizeTimelinePoint) : null,
  }
}

/** Stáhne a znormalizuje nastavení mapy pro dané město. */
export async function loadMapSettings(city: string): Promise<MapModel | null> {
  const response = await fetch(`${import.meta.env.BASE_URL}mapsettings.${encodeURIComponent(city)}.json?v=${Date.now()}`)
  if (!response.ok) return null
  // Vite dev server vrací pro neexistující soubor index.html (SPA fallback) – ten není JSON
  const contentType = response.headers.get('content-type') ?? ''
  if (contentType.includes('text/html')) return null
  return normalizeMapSettings(parseLenientJson(await response.text()))
}
