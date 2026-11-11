// Enumy z EhriMemoMap.Shared a EhriMemoMap.Client/Models.
// Projekt má zapnuté erasableSyntaxOnly, proto místo TS `enum` const objekty + union typy.
// Hodnoty jsou názvy C# enumů (to, co dával C# `.ToString()`), protože v této podobě
// je očekává API (SelectedLayerNames, PlaceType) i Leaflet kód (typy vrstev).

/** EhriMemoMap.Shared.PlaceType */
export const PlaceType = {
  Inaccessible: 'Inaccessible',
  Address: 'Address',
  AddressLastResidence: 'AddressLastResidence',
  Interest: 'Interest',
  Incident: 'Incident',
  Statistics: 'Statistics',
  Memory: 'Memory',
  HeatmapItem: 'HeatmapItem',
  Memorial: 'Memorial',
} as const
export type PlaceType = (typeof PlaceType)[keyof typeof PlaceType]

/** EhriMemoMap.Shared.LayerType */
export const LayerType = {
  Images: 'Images',
  Objects: 'Objects',
  WMS: 'WMS',
  Base: 'Base',
  Polygons: 'Polygons',
  Heatmap: 'Heatmap',
  Narration: 'Narration',
  WFS: 'WFS',
} as const
export type LayerType = (typeof LayerType)[keyof typeof LayerType]

/** EhriMemoMap.Shared.FontColor */
export const FontColor = {
  Light: 'Light',
  Dark: 'Dark',
} as const
export type FontColor = (typeof FontColor)[keyof typeof FontColor]

/** Typ místa příběhové mapy (Place.Type z API) */
export const NarrativeMapStopPlaceType = {
  Main: 'main point',
  Context: 'context point',
  Trajectory: 'trajectory point',
} as const
export type NarrativeMapStopPlaceType = (typeof NarrativeMapStopPlaceType)[keyof typeof NarrativeMapStopPlaceType]

/** EhriMemoMap.Models.DialogTypeEnum – obsah postranního panelu */
export const DialogType = {
  Help: 'Help',
  Layers: 'Layers',
  Place: 'Place',
  Search: 'Search',
  Timeline: 'Timeline',
  None: 'None',
  Welcome: 'Welcome',
  Victim: 'Victim',
  StoryMap: 'StoryMap',
} as const
export type DialogType = (typeof DialogType)[keyof typeof DialogType]

/** EhriMemoMap.Models.MapTypeEnum */
export const MapType = {
  Normal: 'Normal',
  AllStoryMaps: 'AllStoryMaps',
  StoryMapWhole: 'StoryMapWhole',
  StoryMapOneStop: 'StoryMapOneStop',
  /** Osobní trajektorie v detailu osoby – ostatní vrstvy skryté jako u příběhových map */
  Trajectory: 'Trajectory',
} as const
export type MapType = (typeof MapType)[keyof typeof MapType]

/** EhriMemoMap.Client.Services.MapStateEnum */
export const MapState = {
  NotInitialized: 'NotInitialized',
  Initialized: 'Initialized',
  Rendered: 'Rendered',
} as const
export type MapState = (typeof MapState)[keyof typeof MapState]

/** Modální dialogy (Blazor DialogService.Open<Welcome/Error>) */
export const ModalType = {
  None: 'None',
  Welcome: 'Welcome',
  Error: 'Error',
} as const
export type ModalType = (typeof ModalType)[keyof typeof ModalType]

/**
 * Najde hodnotu enumu bez ohledu na velikost písmen (mapsettings.json používá "polygons", "address"…,
 * Newtonsoft to v C# převáděl case-insensitive). Neznámá hodnota → null.
 */
export function parseEnum<T extends string>(values: Record<string, T>, value: unknown): T | null {
  if (typeof value !== 'string' || value === '') return null
  const lower = value.toLowerCase()
  for (const v of Object.values(values)) {
    if (v.toLowerCase() === lower) return v
  }
  return null
}
