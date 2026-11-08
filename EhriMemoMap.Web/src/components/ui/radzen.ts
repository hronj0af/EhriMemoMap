// Pomocné funkce pro komponenty, které vykreslují stejné HTML/CSS třídy jako Radzen.Blazor,
// aby vzhled zůstal shodný (styly dodává radzen-custom/css/material-base.css).

/** Hodnota mezery jako Radzen: číslo → px, jinak beze změny (např. "1rem"). */
export function gapValue(value: string | number | null | undefined): string | null {
  if (value === null || value === undefined || value === '') return null
  return typeof value === 'number' || /^\d+(\.\d+)?$/.test(value) ? `${value}px` : value
}

/** Zarovnání (Radzen AlignItems/JustifyContent) → CSS hodnota použitá v názvu třídy. */
export type FlexAlign =
  | 'normal'
  | 'center'
  | 'start'
  | 'end'
  | 'flex-start'
  | 'flex-end'
  | 'stretch'
  | 'baseline'
  | 'left'
  | 'right'
  | 'space-between'
  | 'space-around'
  | 'space-evenly'

/** Radzen převádí Start/End na flex-start/flex-end. */
export function alignClass(value: FlexAlign | undefined): string {
  if (!value) return 'normal'
  if (value === 'start') return 'flex-start'
  if (value === 'end') return 'flex-end'
  return value
}
