// Vystaví Leaflet jako globální `L` pro pluginy, které ho tak očekávají (leaflet-arrowheads, leaflet-heatmap),
// a aplikuje úpravy z původní (ručně upravené) kopie Leafletu – viz leafletPatches.ts.
import L from 'leaflet'
import { applyLeafletPatches } from './leafletPatches'

declare global {
  interface Window {
    L: typeof L
  }
}

applyLeafletPatches(L)
window.L = L

export { L }
