// Osobní trajektorie (narativní mapa typu 6, Velké Meziříčí) – převod zastavení na body pro mapu.
// Typ zastavení se určuje z názvu zastavení (narrative_map_stops.label_cs), barva podle typu,
// u bydlišť podle toho, zda leží v domovském městě mapy, u místa úmrtí podle druhu místa.
import type { NarrativeMap, NarrativeMapStop } from '../models/api'
import type { TrajectoryStopForLeaflet } from '../models/map'

export type TrajectoryStopKind = 'birth' | 'residence' | 'imprisonment' | 'death' | 'liberation' | 'other'

/** Barvy zastavení (specifikace 11. 11. 2026, odstíny se mohou doladit) */
export const TRAJECTORY_COLORS = {
  birth: '#2E8B3E', // zelená
  residenceHome: '#771646', // vínová (základní barva MemoMapy) – bydliště v domovském městě
  residenceElsewhere: '#2B6CB0', // modrá – bydliště jinde
  imprisonment: '#D7191C', // výrazná červená
  deathImprisonment: '#A86464', // šedo-červená – úmrtí v místě věznění
  death: '#6E6E6E', // šedá – úmrtí jinde
  liberation: '#8CC97A', // světle zelená – místo osvobození
  other: '#6E6E6E',
} as const

/** Typ zastavení podle jeho názvu (česky) */
export function trajectoryStopKind(labelCs: string | null | undefined): TrajectoryStopKind {
  const label = (labelCs ?? '').trim().toLocaleLowerCase('cs')
  if (label.startsWith('místo narození')) return 'birth'
  if (label.startsWith('místo věznění')) return 'imprisonment'
  if (label.startsWith('místo úmrtí')) return 'death'
  if (label.startsWith('místo osvobození')) return 'liberation'
  // Bydliště, Poslední bydliště, Poslední bydliště před deportací
  if (label.includes('bydliště')) return 'residence'
  return 'other'
}

/** Barva zastavení */
export function trajectoryStopColor(stop: NarrativeMapStop, homeTown: string | null): string {
  const place = stop.places?.[0]
  switch (trajectoryStopKind(stop.labelCs)) {
    case 'birth':
      return TRAJECTORY_COLORS.birth
    case 'residence':
      return homeTown != null && place?.townCs === homeTown ? TRAJECTORY_COLORS.residenceHome : TRAJECTORY_COLORS.residenceElsewhere
    case 'imprisonment':
      return TRAJECTORY_COLORS.imprisonment
    case 'death':
      return place?.category === 'imprisonment_place' ? TRAJECTORY_COLORS.deathImprisonment : TRAJECTORY_COLORS.death
    case 'liberation':
      return TRAJECTORY_COLORS.liberation
    default:
      return TRAJECTORY_COLORS.other
  }
}

function escapeHtml(value: string): string {
  return value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;')
}

/** Popisek bodu: název zastavení tučně, adresa/název místa, datace */
export function trajectoryStopTooltip(stop: NarrativeMapStop, en: boolean): string {
  const place = stop.places?.[0]
  const lines = [
    en ? (stop.labelEn ?? stop.labelCs) : stop.labelCs,
    en ? (place?.addressEn ?? place?.addressCs) : place?.addressCs,
    en ? (stop.dateEn ?? stop.dateCs) : stop.dateCs,
  ]
  const [title, address, date] = lines.map((a) => (a ? escapeHtml(a) : null))
  return [title ? `<b>${title}</b>` : null, address, date].filter((a) => a).join('<br/>')
}

/** Zastavení v pořadí stop_order (při shodě podle id) převedená na body pro mapu; zastavení bez místa se vynechají. */
export function toTrajectoryStops(trajectory: NarrativeMap, homeTown: string | null, en: boolean): TrajectoryStopForLeaflet[] {
  const stops = [...(trajectory.stops ?? [])].sort(
    (a, b) => (a.stopOrder ?? Number.MAX_SAFE_INTEGER) - (b.stopOrder ?? Number.MAX_SAFE_INTEGER) || a.id - b.id,
  )
  const result: TrajectoryStopForLeaflet[] = []
  for (const stop of stops) {
    const mapPoint = stop.places?.[0]?.mapPoint
    if (!mapPoint) continue
    let coordinates: number[] | undefined
    try {
      coordinates = (JSON.parse(mapPoint) as { coordinates?: number[] }).coordinates
    } catch {
      continue
    }
    if (!coordinates || coordinates.length < 2) continue
    result.push({
      lat: coordinates[1],
      lng: coordinates[0],
      color: trajectoryStopColor(stop, homeTown),
      tooltip: trajectoryStopTooltip(stop, en),
    })
  }
  return result
}
