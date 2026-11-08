// Filtr obětí podle jména (podmínka opakovaná v Address/PragueVictimList/PacovVictimList/RicanyVictimList.razor).
import type { VictimShortInfoModel } from '../../../../models/api'

/** Odpovídá oběť filtru? Prázdný filtr → vždy ano. */
export function matchesFilter(victim: VictimShortInfoModel, filteredName: string | null | undefined): boolean {
  if (!filteredName) return true
  return victim.label != null && victim.label.toLowerCase().includes(filteredName.toLowerCase())
}

/** Je ve skupině aspoň jedna oběť odpovídající filtru? (prázdný filtr → ano) */
export function anyMatchesFilter(victims: VictimShortInfoModel[] | null | undefined, filteredName: string | null | undefined): boolean {
  if (!filteredName) return true
  return victims?.some((a) => matchesFilter(a, filteredName)) ?? false
}
