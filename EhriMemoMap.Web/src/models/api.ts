// Datové modely vracené / přijímané API (EhriMemoMap.Shared/MapModels a Parameters).
// Názvy vlastností jsou camelCase – tak je serializuje minimal API (System.Text.Json, JsonSerializerDefaults.Web).
// Odvozené vlastnosti/metody C# tříd (GetTitle, GetLabelForPicture, Statistics…) jsou funkce v helpers.ts.

/** DateTime z API – ISO řetězec bez časové zóny, např. "1942-04-01T00:00:00". Parsovat přes helpers.parseApiDate. */
export type ApiDate = string

/** GeoJSON geometrie (NetTopologySuite Geometry serializovaná GeoJsonConverterFactory) */
export interface GeoJsonGeometry {
  type: string
  coordinates: unknown
}

export interface PointModel {
  x: number
  y: number
}

export interface OmekaUrl {
  url: string | null
  thumbnailUrl: string | null
}

export interface Document {
  id: number
  type: string | null
  labelCs: string | null
  labelEn: string | null
  descriptionCs: string | null
  descriptionEn: string | null
  creationDateCs: string | null
  creationDateEn: string | null
  creationPlaceCs: string | null
  creationPlaceEn: string | null
  ownerCs: string | null
  ownerEn: string | null
  url: OmekaUrl[] | null
  documentUrlCs: string | null
  documentUrlEn: string | null
}

export interface MapObject {
  placeType: string | null
  layerName: string | null
  citizens: number | null
  citizensTotal: number | null
  id: number | null
  labelCs: string | null
  labelEn: string | null
  geographyMapPoint: GeoJsonGeometry | null
  geographyMapPolygon: GeoJsonGeometry | null
  /** GeoJSON jako řetězec – s ním pracuje Leaflet kód */
  mapPoint: string | null
  mapPolygon: string | null
  dateFrom: ApiDate | null
  dateTo: ApiDate | null
}

export interface MapStatistic {
  id: number | null
  type: string | null
  quarterCs: string | null
  quarterEn: string | null
  count: number | null
  geography: GeoJsonGeometry | null
  geographyPolygon: GeoJsonGeometry | null
  mapPolygon: string | null
  mapPoint: string | null
  dateFrom: ApiDate | null
  dateTo: ApiDate | null
}

export interface Place {
  id: number
  type: string
  labelCs: string | null
  labelEn: string | null
  addressCs: string | null
  addressEn: string | null
  townCs: string | null
  townEn: string | null
  streetCs: string | null
  streetEn: string | null
  houseNr: string | null
  remarkCs: string | null
  remarkEn: string | null
  mapPoint: string | null
  stopId: number | null
  narrativeMapId: number | null
  /** Druh místa (places.type: house, town, imprisonment_place…) – jen u zastavení narativních map */
  category?: string | null
  documents: Document[] | null
}

export interface DescriptionSection {
  title: string | null
  text: string | null
}

export interface NarrativeMapStop {
  id: number
  stopOrder: number | null
  labelCs: string | null
  labelEn: string | null
  dateCs: string | null
  dateEn: string | null
  descriptionCs: string | null
  descriptionEn: string | null
  descriptionSectionsCs: DescriptionSection[] | null
  descriptionSectionsEn: DescriptionSection[] | null
  documents: Document[] | null
  places: Place[] | null
}

export interface NarrativeMap {
  id: number
  type: number
  labelCs: string | null
  labelEn: string | null
  descriptionCs: string | null
  descriptionEn: string | null
  mainPoints: Place[] | null
  stops: NarrativeMapStop[] | null
}

export interface PlaceIncident {
  id: number
  labelCs: string | null
  labelEn: string | null
  typeCs: string | null
  typeEn: string | null
  specificationCs: string | null
  specificationEn: string | null
  addressCs: string | null
  addressEn: string | null
  descriptionCs: string | null
  descriptionEn: string | null
  dateCs: string | null
  dateEn: string | null
  date: ApiDate | null
  documents: Document[] | null
}

export interface PlaceInterest {
  labelCs: string | null
  labelEn: string | null
  addressCs: string | null
  addressEn: string | null
  descriptionCs: string | null
  descriptionEn: string | null
  narrativeMapId: number | null
  documents: Document[] | null
}

export interface PlaceMemorial {
  id: number
  labelCs: string | null
  labelEn: string | null
  linkCs: string | null
  linkEn: string | null
  addressCs: string | null
  addressEn: string | null
  descriptionCs: string | null
  descriptionEn: string | null
  date: ApiDate | null
  documents: Document[] | null
}

export interface PlaceMemoryItem {
  id: number
  labelCs: string | null
  labelEn: string | null
  inscriptionCs: string | null
  inscriptionEn: string | null
  creationDate: ApiDate | null
  linkStolpersteineCs: string | null
  linkStolpersteineEn: string | null
  linkHolocaustCs: string | null
  linkHolocaustEn: string | null
}

export interface PlaceMemory {
  type: string | null
  city: string | null
  labelCs: string | null
  labelEn: string | null
  addressCs: string | null
  addressEn: string | null
  descriptionCs: string | null
  descriptionEn: string | null
  inscriptionCs: string | null
  inscriptionEn: string | null
  creationDate: ApiDate | null
  items: PlaceMemoryItem[] | null
  documents: Document[] | null
}

export interface AddressInfo {
  id: number
  cs: string | null
  en: string | null
  de: string | null
  currentCs: string | null
  currentEn: string | null
  type: number | null
  typeCs: string | null
  typeEn: string | null
  dateFrom: ApiDate | null
  dateTo: ApiDate | null
}

export interface AddressStatistics {
  date: ApiDate
  numberAbsent: number | null
  numberPresent: number | null
}

/** Pražský dům – počty přítomných a deportovaných k jednotlivým datům (presentYYYYMMDD / deportedYYYYMMDD). */
export interface PragueAddressesStatsTimeline {
  id: number
  addressCs: string | null
  addressEn: string | null
  addressDe: string | null
  count: number | null
  murdered: number | null
  survived: number | null
  fateUnknown: number | null
  addressCurrentCs: string | null
  addressCurrentEn: string | null
  addressCurrentDe: string | null
  geography: GeoJsonGeometry | null
  [key: `present${number}`]: number | null
  [key: `deported${number}`]: number | null
}

export interface VictimShortInfoModel {
  id: number
  label: string | null
  name: string | null
  birthdate: string | null
  detailsCs: string | null
  detailsEn: string | null
  photo: string | null
  narrativeMapId: number | null
  transportDate: ApiDate | null
  relationshipToAddressType: number | null
  relationshipToPersonType: number | null
  relationshipToPersonCs: string | null
  relationshipToPersonEn: string | null
  longInfo: boolean
  documents: Document[] | null
}

export interface AddressWithVictims {
  address: AddressInfo | null
  pragueAddress: PragueAddressesStatsTimeline | null
  victims: VictimShortInfoModel[] | null
}

export interface PlacesResult {
  incidents: PlaceIncident[] | null
  placesOfInterest: PlaceInterest[] | null
  inaccessiblePlaces: PlaceInterest[] | null
  addresses: AddressWithVictims[] | null
  addressesLastResidence: AddressWithVictims[] | null
  placesOfMemory: PlaceMemory[] | null
  memorials: PlaceMemorial[] | null
}

export interface Transport {
  id: number
  date: ApiDate | null
  code: string | null
  fromCs: string | null
  fromEn: string | null
  toCs: string | null
  toEn: string | null
}

export interface VictimLongInfoModel {
  id: number
  birthDate: ApiDate | null
  deathDate: ApiDate | null
  birthDateText: string | null
  deathDateText: string | null
  label: string | null
  maidenname: string | null
  title: string | null
  fateCs: string | null
  fateEn: string | null
  descriptionCs: string | null
  descriptionEn: string | null
  photo: string | null
  places: AddressInfo[] | null
  relatedPersons: VictimShortInfoModel[] | null
  documents: Document[] | null
  transports: Transport[] | null
  transportDate: ApiDate | null
  /** Příběhová mapa (typ 5) */
  narrativeMapId: number | null
  /** Osobní trajektorie (narativní mapa typu 6) */
  trajectoryId?: number | null
}

export interface SolrPlace {
  all: string | null
  city: string | null
  id: string
  detailId: number | null
  labelCs: string | null
  labelEn: string | null
  mapLocation: string | null
  mapObject: string | null
  placeCs: string | null
  placeCurrentCs: string | null
  placeCurrentDe: string | null
  placeCurrentEn: string | null
  placeDate: string[] | null
  placeDe: string | null
  placeEn: string | null
  type: string | null
  date: ApiDate | null
}

export interface WelcomeDialogStatistics {
  victims: number | null
  incidents: number | null
  interests: number | null
  inaccessibles: number | null
  placesOfMemory: number | null
  memorials: number | null
  storyMaps: number | null
}

// ---------- Parametry požadavků ----------

/** Bod časové osy tak, jak se posílá v MapObjectParameters (jen pole, která API používá). */
export interface TimelinePointParameter {
  name?: string | null
  title?: string | null
  selected?: boolean
  from: ApiDate | null
  to: ApiDate | null
}

export interface MapObjectParameters {
  city: string | null
  customCoordinates: PointModel[] | null
  mapSouthWestPoint: PointModel | null
  mapNorthEastPoint: PointModel | null
  selectedLayerNames: (string | null)[] | null
  selectedTimeLinePoint: TimelinePointParameter | null
}

export interface DistrictStatisticsParameters {
  city: string | null
  total: boolean
  /** yyyy-MM-dd */
  timeLinePoint: string | null
}

export interface PlacesParameters {
  city: string | null
  memorialsIds?: number[] | null
  incidentsIds?: number[] | null
  placesOfInterestIds?: number[] | null
  placesOfMemoryIds?: number[] | null
  inaccessiblePlacesIds?: number[] | null
  addressesIds?: number[] | null
  addressesLastResidenceIds?: number[] | null
}

export interface SolrQueryParameters {
  city: string | null
  query: string
}

export interface WFSLayerInfo {
  url: string | null
  placeType: string | null
  name: string | null
}

export interface WFSParameters {
  wfsLayers: WFSLayerInfo[] | null
}
