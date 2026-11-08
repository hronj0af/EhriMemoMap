// Úpravy Leafletu převzaté z ručně upraveného EhriMemoMap.Client/wwwroot/js/leaflet-src.js (Leaflet 1.9.4).
// Blazor verze používala vlastní kopii Leafletu s těmito změnami; tady se aplikují na npm Leaflet 1.9.4,
// aby se mapa chovala stejně.
import type * as Leaflet from 'leaflet'

type LeafletNs = typeof Leaflet

export function applyLeafletPatches(L: LeafletNs): void {
  // 1) Výchozí parametry WMS – PNG s průhledností (pod historickou mapou je vidět OSM), WMS 1.3.0
  //    a parametry pro QGIS server (tiled, dpi, opacities, map).
  const wmsProto = (L.TileLayer.WMS.prototype as unknown as { defaultWmsParams: Record<string, unknown> })
  Object.assign(wmsProto.defaultWmsParams, {
    format: 'image/png',
    transparent: true,
    version: '1.3.0',
    srs: 'EPSG:3857',
    crs: 'EPSG:3857',
    tiled: true,
    dpi: 96,
    opacities: '255,255,217,255,205,255',
    map: '',
  })

  // 2) CRS: při zoomu 0 se nepřevádí na pixely (latLngToPoint vrací projektované souřadnice a pointToLatLng je bere).
  //    Leaflet kopíruje metody CRS do jednotlivých CRS objektů (Util.extend), proto se patchují všechny.
  type Crs = {
    latLngToPoint(latlng: Leaflet.LatLngExpression, zoom: number): Leaflet.Point
    pointToLatLng(point: Leaflet.PointExpression, zoom: number): Leaflet.LatLng
    projection: Leaflet.Projection
    __memomapPatched?: boolean
  }
  const crsList = [L.CRS.EPSG3857, L.CRS.EPSG3395, L.CRS.EPSG4326, L.CRS.Earth, L.CRS.Simple, L.CRS.EPSG900913] as unknown as Crs[]
  for (const crs of crsList) {
    if (!crs || crs.__memomapPatched) continue
    const latLngToPoint = crs.latLngToPoint
    const pointToLatLng = crs.pointToLatLng
    crs.latLngToPoint = function (this: Crs, latlng, zoom) {
      if (zoom == 0) return this.projection.project(L.latLng(latlng as Leaflet.LatLngExpression))
      return latLngToPoint.call(this, latlng, zoom)
    }
    crs.pointToLatLng = function (this: Crs, point, zoom) {
      if (zoom == 0) return this.projection.unproject(L.point(point as Leaflet.PointExpression))
      return pointToLatLng.call(this, point, zoom)
    }
    crs.__memomapPatched = true
  }

  // 3) GridLayer.getTileSize: nekladná velikost dlaždice → velikost okna.
  const gridProto = L.GridLayer.prototype as unknown as { getTileSize(): Leaflet.Point; options: { tileSize: number | Leaflet.Point } }
  gridProto.getTileSize = function (this: typeof gridProto) {
    const s = this.options.tileSize
    if (s instanceof L.Point) return s
    return s > 0 ? new L.Point(s, s) : new L.Point(window.outerWidth, window.outerHeight)
  }
}
