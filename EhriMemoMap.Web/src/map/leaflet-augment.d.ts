// Rozšíření typů Leafletu o metodu z pluginu leaflet-arrowheads.
import 'leaflet'

declare module 'leaflet' {
  interface ArrowheadOptions {
    frequency?: number | string
    fill?: boolean
    size?: string
    color?: string
    yawn?: number
  }
  interface Polyline {
    arrowheads(options?: ArrowheadOptions): this
  }
}
