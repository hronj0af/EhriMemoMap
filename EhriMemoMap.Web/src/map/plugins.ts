// Leaflet + pluginy ve správném pořadí.
// leaflet-arrowheads (a leaflet-heatmap) očekávají globální `L` už při načtení, proto ho nejdřív
// vystaví leafletGlobal.ts. ES importy se vyhodnocují v pořadí zápisu.
// leaflet-arrowheads je z npm; heatmap.js 2.0.5 + leaflet-heatmap jsou v vendor/ jako ES moduly s opravou
// pro strict mode (viz hlavička vendor/heatmap.js). Vše jde do bundlu, takže to legacy build převede pro staré prohlížeče.
import { L } from './leafletGlobal'
import 'leaflet-arrowheads'
import HeatmapOverlayModule from './vendor/leaflet-heatmap.js'

export interface HeatmapOverlayConfig {
  radius?: number
  maxOpacity?: number
  scaleRadius?: boolean
  useLocalExtrema?: boolean
  latField?: string
  lngField?: string
  valueField?: string
}

export interface HeatmapData {
  max: number
  data: { lat: number; lng: number; count: number }[]
}

export interface HeatmapOverlayLayer extends L.Layer {
  setData(data: HeatmapData): void
}

export const HeatmapOverlay = HeatmapOverlayModule as unknown as new (config: HeatmapOverlayConfig) => HeatmapOverlayLayer

export { L }
