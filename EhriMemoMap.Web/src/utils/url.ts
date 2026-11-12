// Práce s parametry URL (z site.ts: getUrlParam, setUrlParam, getSelectedPlaceFromUrl…).
// Parametry: layers, timeline, bounds, zoom, x1/y1/x2/y2 (vybrané místo), victim (otevřený detail osoby).
import type { Coordinate } from '../models/map'

export function getUrlParam(name: string): string | null {
  return new URLSearchParams(window.location.search).get(name)
}

/**
 * Nastaví parametr URL bez nové položky v historii.
 * Zachovává history.state – site.ts volal replaceState({}), což by mazalo stav vue-routeru.
 */
export function setUrlParam(name: string, value: string | number | null | undefined): void {
  const params = new URLSearchParams(window.location.search)
  params.set(name, value == null ? '' : String(value))
  history.replaceState(history.state, '', window.location.pathname + '?' + params.toString())
}

/** Odstraní parametr z URL (bez nové položky v historii). */
export function removeUrlParam(name: string): void {
  const params = new URLSearchParams(window.location.search)
  if (!params.has(name)) return
  params.delete(name)
  const query = params.toString()
  history.replaceState(history.state, '', window.location.pathname + (query ? '?' + query : ''))
}

/** Vybrané místo z URL (x1, y1, x2, y2) – getSelectedPlaceFromUrl. Neúplné → null. */
export function getSelectedPlaceFromUrl(): [Coordinate, Coordinate] | null {
  const x1 = getUrlParam('x1')
  const y1 = getUrlParam('y1')
  const x2 = getUrlParam('x2')
  const y2 = getUrlParam('y2')
  if (x1 == null || y1 == null || x2 == null || y2 == null) return null
  const result: [Coordinate, Coordinate] = [
    { x: parseFloat(x1), y: parseFloat(y1) },
    { x: parseFloat(x2), y: parseFloat(y2) },
  ]
  return result.some((c) => Number.isNaN(c.x) || Number.isNaN(c.y)) ? null : result
}

/** Zapíše vybrané místo do URL – setUrlFromSelectedPlace. */
export function setUrlFromSelectedPlace(points: Coordinate[]): void {
  if (!points || points.length !== 2) return
  setUrlParam('x1', points[0].x)
  setUrlParam('y1', points[0].y)
  setUrlParam('x2', points[1].x)
  setUrlParam('y2', points[1].y)
}
