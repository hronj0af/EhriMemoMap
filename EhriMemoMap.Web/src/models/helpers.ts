// Pomocné funkce – náhrada odvozených vlastností a metod C# modelů (EhriMemoMap.Shared)
// a helperů z EhriMemoMap.Client/Helpers (StringHelpers, ParseHelpers, ComparerHelpers).
import type {
  AddressInfo,
  AddressStatistics,
  AddressWithVictims,
  ApiDate,
  Document,
  SolrPlace,
  VictimLongInfoModel,
  VictimShortInfoModel,
} from './api'
import type { Culture } from '../i18n'
import type { Coordinate } from './map'

// ---------------- Datumy ----------------

export interface DateParts {
  year: number
  month: number
  day: number
}

/**
 * Rozloží datum z API ("1942-04-01T00:00:00", "1942-04-01") na části bez převodu přes časovou zónu
 * (new Date("1942-04-01") by v JS bylo UTC a v některých zónách by vyšel předchozí den).
 */
export function parseApiDate(value: ApiDate | null | undefined): DateParts | null {
  if (!value) return null
  const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(value)
  if (!m) return null
  return { year: Number(m[1]), month: Number(m[2]), day: Number(m[3]) }
}

/** Datum ve formátu C# "d. M. yyyy" (např. "1. 4. 1942"). Neplatné / prázdné → "". */
export function formatDate(value: ApiDate | null | undefined): string {
  const d = parseApiDate(value)
  return d ? `${d.day}. ${d.month}. ${d.year}` : ''
}

/** Normalizovaný klíč data "yyyy-MM-dd" pro porovnávání (náhrada porovnání DateTime). */
export function dateKey(value: ApiDate | null | undefined): string | null {
  const d = parseApiDate(value)
  if (!d) return null
  return `${d.year}-${String(d.month).padStart(2, '0')}-${String(d.day).padStart(2, '0')}`
}

// ---------------- Řetězce (StringHelpers) ----------------

/** Nahradí konce řádků za <br/> (StringHelpers.Nl2Br). */
export function nl2br(text: string | null | undefined): string {
  return text ? text.replace(/\n/g, '<br/>') : ''
}

/** První písmeno velké (StringHelpers.FirstLetterToUpper). */
export function firstLetterToUpper(text: string | null | undefined): string {
  if (!text) return text ?? ''
  return text.charAt(0).toUpperCase() + text.substring(1)
}

/** Převede URL a e-maily v textu na odkazy (StringHelpers.Linkify). */
export function linkify(text: string): string {
  let result = text.replace(
    /((www\.|(http|https|ftp|news|file)+:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9/&#95;:@=.+?,##%&~-]*[^.|'|# |!|(|?|,| |>|<|;|)])/gi,
    '<a href="$1" target="_blank">$1</a>',
  )
  result = result.split('href="www').join('href="http://www')
  result = result.replace(/(([a-zA-Z0-9_\-.])+@[a-zA-Z ]+?(\.[a-zA-Z]{2,6})+)/gi, '<a href="mailto:$1">$1</a>')
  return result
}

/** Převod českých názvů atributů v XML na parsovatelné hodnoty (ParseHelpers.ConvertXMLNameAttributesToParsableValues). */
const NAME_ATTRIBUTE_MAPPING: Record<string, string> = {
  'Jméno (datum narození)': 'NameAndBirthDate',
  Detaily: 'Details',
  Adresa: 'Address',
  'Adresa (česky, z doby okupace)': 'AddressCzechOccupation',
  'Adresa (německy, z doby okupace)': 'AddressGermanOccupation',
  'Počet židovských obyvatel (1. 10. 1941)': 'JewsCitizens1942',
  Zavražděno: 'Murdered',
  Přežilo: 'Survived',
  'Osud neznámý': 'FateUnknown',
  'Nedeportováno k 1. 1. 1942': 'NotDeportedUntil_1_1_1942',
  'Nedeportováno k 1. 1. 1943': 'NotDeportedUntil_1_1_1943',
  'Nedeportováno k 1. 1. 1944': 'NotDeportedUntil_1_1_1944',
  'Nedeportováno k 1. 1. 1945': 'NotDeportedUntil_1_1_1945',
  'Nedeportováno k 9. 5. 1945': 'NotDeportedUntil_9_5_1945',
  Název: 'Title',
  Popis: 'Description',
  Typ: 'Type',
  Specifikace: 'Specification',
  'Typ (alternativní)': 'TypeAlternative',
  'Specifikace (alternativní)': 'SpecificationAlternative',
  Datum: 'Date',
  Místo: 'Place',
  Dokumenty: 'Documents',
}

export function convertXmlNameAttributesToParsableValues(text: string | null | undefined): string | null {
  if (!text) return null
  let result = text
  for (const [from, to] of Object.entries(NAME_ATTRIBUTE_MAPPING)) {
    result = result.split(`name="${from}"`).join(`name="${to}"`)
  }
  return result
}

// ---------------- Odvozené vlastnosti modelů ----------------

/** Document.GetTitle(lang) – popisek dokumentu do galerie. */
export function documentTitle(doc: Document, culture: Culture): string {
  const parts =
    culture === 'cs-CZ'
      ? [doc.descriptionCs, doc.creationDateCs, doc.creationPlaceCs, doc.ownerCs]
      : [doc.descriptionEn, doc.creationDateEn, doc.creationPlaceEn, doc.ownerEn]
  const label = (culture === 'cs-CZ' ? doc.labelCs : doc.labelEn) ?? ''
  return label + parts.filter((p) => !!p).map((p) => ' | ' + p).join('')
}

/** SolrPlace.DropDownInfoCs / DropDownInfoEn */
export function solrDropDownInfo(place: SolrPlace, culture: Culture): string {
  if (culture === 'en-US') {
    const suffix = place.placeEn ? ' | ' + place.placeEn : place.placeCs ? ' | ' + place.placeCs : ''
    return (place.labelEn ?? '') + suffix
  }
  return (place.labelCs ?? '') + (place.placeCs ? ' | ' + place.placeCs : '')
}

/** Souřadnice bodu z GeoJSON stringu ({"type":"Point","coordinates":[lng,lat]}); jinak null */
export function parseGeoJsonPoint(mapPoint: string | null | undefined): Coordinate | null {
  if (!mapPoint) return null
  try {
    const coordinates = (JSON.parse(mapPoint) as { coordinates?: unknown }).coordinates
    if (!Array.isArray(coordinates) || coordinates.length < 2) return null
    const [x, y] = coordinates as unknown[]
    return typeof x === 'number' && typeof y === 'number' && Number.isFinite(x) && Number.isFinite(y) ? { x, y } : null
  } catch {
    return null
  }
}

/** AddressInfo.GetAddress(cultureName) */
export function addressText(address: AddressInfo, culture: Culture): string | null {
  return culture === 'en-US' ? address.en : address.cs
}

/** AddressInfo.GetAddressWithDates(cultureName) – adresa s obdobím, např. "Dlouhá 5 (1939-1941)". */
export function addressWithDates(address: AddressInfo, culture: Culture): string {
  const text = (culture === 'en-US' ? address.en : address.cs) ?? ''
  const dates = addressDates(address, culture)
  return dates ? `${text} ${dates}` : text
}

/** Jen období z addressWithDates, např. "(1939-1941)", "(do 1941)"; bez datací prázdný řetězec. */
export function addressDates(address: AddressInfo, culture: Culture): string {
  const en = culture === 'en-US'
  const from = parseApiDate(address.dateFrom)
  const to = parseApiDate(address.dateTo)
  if (!from && !to) return ''
  if (from && to) return from.year !== to.year ? `(${from.year}-${to.year})` : `(${from.year})`
  if (to) return `(${en ? 'until ' : 'do '}${to.year})`
  return `(${en ? 'after ' : 'po '}${from!.year})`
}

/**
 * Datace bydliště na úrovni roků (Velké Meziříčí): (1938), (1938 – 1940), (1938 – ?), (? – 1938), (?).
 * Oba údaje ve stejném roce → jen rok; chybějící hranice → otazník.
 */
export function residenceYears(dateFrom: ApiDate | null | undefined, dateTo: ApiDate | null | undefined): string {
  const from = parseApiDate(dateFrom)?.year ?? null
  const to = parseApiDate(dateTo)?.year ?? null
  if (from == null && to == null) return '(?)'
  if (from != null && to != null && from === to) return `(${from})`
  return `(${from ?? '?'} – ${to ?? '?'})`
}

interface DatedRecord {
  id: number
  dateFrom: ApiDate | null
  dateTo: ApiDate | null
}

/**
 * Chronologické řazení bydlišť (Velké Meziříčí):
 * 1. záznamy s date_from vzestupně podle date_from,
 * 2. záznam jen s date_to se zařadí za poslední záznam, jehož date_from je starší než jeho date_to
 *    (když takový není, na začátek),
 * 3. záznamy bez obou datací na konec,
 * 4. při shodě rozhoduje id vztahu.
 */
export function sortResidencesChronologically<T extends DatedRecord>(items: readonly T[]): T[] {
  const compare = (a: string, b: string) => (a < b ? -1 : a > b ? 1 : 0)
  const withFrom = items
    .filter((a) => dateKey(a.dateFrom) != null)
    .sort((a, b) => compare(dateKey(a.dateFrom)!, dateKey(b.dateFrom)!) || a.id - b.id)
  const onlyTo = items
    .filter((a) => dateKey(a.dateFrom) == null && dateKey(a.dateTo) != null)
    .sort((a, b) => compare(dateKey(a.dateTo)!, dateKey(b.dateTo)!) || a.id - b.id)
  const undated = items.filter((a) => dateKey(a.dateFrom) == null && dateKey(a.dateTo) == null).sort((a, b) => a.id - b.id)

  // index záznamu s date_from, za který se záznam jen s date_to zařadí (-1 = před první)
  const inserted = new Map<number, T[]>()
  for (const item of onlyTo) {
    const to = dateKey(item.dateTo)!
    let anchor = -1
    withFrom.forEach((a, i) => {
      if (dateKey(a.dateFrom)! < to) anchor = i
    })
    inserted.set(anchor, [...(inserted.get(anchor) ?? []), item])
  }

  const result: T[] = [...(inserted.get(-1) ?? [])]
  withFrom.forEach((a, i) => result.push(a, ...(inserted.get(i) ?? [])))
  return [...result, ...undated]
}

/** Label ve tvaru "<b>Jméno</b><br/>*datum" (VictimShortInfoModel.GetShortLabelForPicture / VictimLongInfoModel.GetLabelForPicture). */
function boldLabel(label: string | null): string {
  if (!label) return ''
  return ('<b>' + label).replace(' (*', '</b><br/>*').split(')').join('')
}

export function victimLongLabelForPicture(victim: VictimLongInfoModel): string {
  return boldLabel(victim.label)
}

/** VictimShortInfoModel.GetLabelForPicture() */
export function victimLabelForPicture(victim: VictimShortInfoModel): string {
  return victim.label ? boldLabel(victim.label) : (victim.name ?? '') + (victim.birthdate != null ? ' (*' + victim.birthdate + ')' : '')
}

/** VictimShortInfoModel.GetShortLabelForPicture() */
export function victimShortLabelForPicture(victim: VictimShortInfoModel): string {
  return boldLabel(victim.label)
}

/** VictimShortInfoModel.GetBirthdateTitle() */
export function victimBirthdateTitle(victim: VictimShortInfoModel): string {
  return victim.birthdate != null ? '* ' + victim.birthdate : ''
}

/** Data, ke kterým jsou v PragueAddressesStatsTimeline počty přítomných/deportovaných (AddressWithVictims.Statistics). */
const PRAGUE_STATISTICS_DATES = [
  '19420101', '19420401', '19420701', '19421001',
  '19430101', '19430401', '19430701', '19431001',
  '19440101', '19440401', '19440701', '19441001',
  '19450101', '19450401', '19450508',
] as const

const statisticsCache = new WeakMap<AddressWithVictims, AddressStatistics[]>()

/** AddressWithVictims.Statistics – počty přítomných a deportovaných k jednotlivým datům (jen pražské adresy). */
export function addressStatistics(house: AddressWithVictims): AddressStatistics[] {
  const prague = house.pragueAddress
  if (!prague) return []
  const cached = statisticsCache.get(house)
  if (cached) return cached
  const result = PRAGUE_STATISTICS_DATES.map((d) => ({
    date: `${d.substring(0, 4)}-${d.substring(4, 6)}-${d.substring(6, 8)}`,
    numberPresent: prague[`present${Number(d)}`] ?? null,
    numberAbsent: prague[`deported${Number(d)}`] ?? null,
  }))
  statisticsCache.set(house, result)
  return result
}

/** Odstraní duplicitní oběti podle Id (ComparerHelpers.VictimComparer + Distinct). */
export function distinctVictims(victims: VictimShortInfoModel[]): VictimShortInfoModel[] {
  const seen = new Set<number>()
  return victims.filter((v) => (seen.has(v.id) ? false : (seen.add(v.id), true)))
}
