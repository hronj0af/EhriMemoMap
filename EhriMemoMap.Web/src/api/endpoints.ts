// Volání jednotlivých metod EhriMemoMap.API (viz EhriMemoMap.API/Program.cs).
import { apiGet, apiPost, type RequestOptions } from './client'
import type {
  DistrictStatisticsParameters,
  MapObject,
  MapObjectParameters,
  MapStatistic,
  NarrativeMap,
  PlacesParameters,
  PlacesResult,
  SolrPlace,
  SolrQueryParameters,
  VictimLongInfoModel,
  WelcomeDialogStatistics,
  WFSParameters,
} from '../models/api'

export function getMapObjects(parameters: MapObjectParameters, options?: RequestOptions) {
  return apiPost<MapObject[] | null>('getmapobjects', parameters, options)
}

export function getWfsObjects(parameters: WFSParameters, options?: RequestOptions) {
  return apiPost<MapObject[] | null>('getwfsobjects', parameters, options)
}

export function getHeatmap(parameters: MapObjectParameters, options?: RequestOptions) {
  return apiPost<MapObject[] | null>('getheatmap', parameters, options)
}

export function getDistrictStatistics(parameters: DistrictStatisticsParameters, options?: RequestOptions) {
  return apiGet<MapStatistic[] | null>(
    'getdistrictstatistics',
    { city: parameters.city, total: parameters.total, timeLinePoint: parameters.timeLinePoint },
    options,
  )
}

export function getPlaces(parameters: PlacesParameters, options?: RequestOptions) {
  return apiPost<PlacesResult | null>('getplaces', parameters, options)
}

export function getSolrPlaces(parameters: SolrQueryParameters, options?: RequestOptions) {
  return apiPost<SolrPlace[] | null>('getsolrplaces', parameters, options)
}

export function getVictimLongInfo(city: string | null | undefined, id: number, options?: RequestOptions) {
  return apiGet<VictimLongInfoModel | null>('getvictimlonginfo', { city, id }, options)
}

export function getAllNarrativeMaps(city: string | null | undefined, options?: RequestOptions) {
  return apiGet<NarrativeMap[] | null>('getallnarrativemaps', { city }, options)
}

export function getNarrativeMap(id: number, city: string | null | undefined, options?: RequestOptions) {
  return apiGet<NarrativeMap | null>('getnarrativemap', { id, city }, options)
}

export function getWelcomeDialogStatistics(city: string | null | undefined, options?: RequestOptions) {
  return apiGet<WelcomeDialogStatistics | null>('getwelcomedialogstatistics', { city }, options)
}
