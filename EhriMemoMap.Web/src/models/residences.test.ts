import { describe, expect, it } from 'vitest'
import { residenceYears, sortResidencesChronologically } from './helpers'

describe('residenceYears – datace bydliště na úrovni roků (specifikace, tabulka příkladů)', () => {
  it.each([
    ['1938-09-25T00:00:00', '1938-10-03T00:00:00', '(1938)'],
    ['1938-09-25T00:00:00', '1940-02-10T00:00:00', '(1938 – 1940)'],
    ['1938-09-25T00:00:00', null, '(1938 – ?)'],
    [null, '1938-08-31T00:00:00', '(? – 1938)'],
    [null, null, '(?)'],
  ])('%s / %s → %s', (from, to, expected) => {
    expect(residenceYears(from, to)).toBe(expected)
  })
})

const r = (id: number, dateFrom: string | null, dateTo: string | null) => ({ id, dateFrom, dateTo })
const ids = (items: { id: number }[]) => items.map((a) => a.id)

describe('sortResidencesChronologically', () => {
  it('řadí podle date_from vzestupně', () => {
    const items = [r(1, '1941-01-01', null), r(2, '1938-01-01', null), r(3, '1939-06-01', '1940-01-01')]
    expect(ids(sortResidencesChronologically(items))).toEqual([2, 3, 1])
  })

  it('záznam jen s date_to zařadí za poslední záznam se starším date_from', () => {
    const items = [r(1, '1938-01-01', null), r(2, '1940-01-01', null), r(3, '1942-01-01', null), r(4, null, '1941-05-01')]
    expect(ids(sortResidencesChronologically(items))).toEqual([1, 2, 4, 3])
  })

  it('záznam jen s date_to starším než všechna date_from jde na začátek', () => {
    expect(ids(sortResidencesChronologically([r(1, '1938-01-01', null), r(2, null, '1930-01-01')]))).toEqual([2, 1])
  })

  it('date_from shodné s date_to není starší – záznam jde před něj', () => {
    const items = [r(1, '1938-01-01', null), r(2, '1940-01-01', null), r(3, null, '1940-01-01')]
    expect(ids(sortResidencesChronologically(items))).toEqual([1, 3, 2])
  })

  it('víc záznamů jen s date_to na stejném místě – podle date_to', () => {
    const items = [r(1, '1938-01-01', null), r(2, null, '1939-05-01'), r(3, null, '1939-01-01'), r(4, '1945-01-01', null)]
    expect(ids(sortResidencesChronologically(items))).toEqual([1, 3, 2, 4])
  })

  it('záznamy bez datací na konec, podle id', () => {
    const items = [r(5, null, null), r(1, '1938-01-01', null), r(2, null, null), r(3, null, '1937-01-01')]
    expect(ids(sortResidencesChronologically(items))).toEqual([3, 1, 2, 5])
  })

  it('při shodných datech rozhoduje id', () => {
    expect(ids(sortResidencesChronologically([r(9, '1938-01-01', null), r(4, '1938-01-01', '1939-01-01')]))).toEqual([4, 9])
  })

  it('nemění vstupní pole', () => {
    const items = [r(2, '1940-01-01', null), r(1, '1938-01-01', null)]
    sortResidencesChronologically(items)
    expect(ids(items)).toEqual([2, 1])
  })
})
