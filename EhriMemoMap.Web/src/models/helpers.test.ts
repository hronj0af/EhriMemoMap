import { describe, expect, it } from 'vitest'
import {
  addressStatistics,
  addressWithDates,
  dateKey,
  documentTitle,
  firstLetterToUpper,
  formatDate,
  linkify,
  nl2br,
  solrDropDownInfo,
  victimBirthdateTitle,
  victimLabelForPicture,
  victimShortLabelForPicture,
} from './helpers'
import type { AddressInfo, AddressWithVictims, Document, SolrPlace, VictimShortInfoModel } from './api'

describe('datumy', () => {
  it('formatDate jako C# "d. M. yyyy" bez posunu časové zóny', () => {
    expect(formatDate('1942-04-01T00:00:00')).toBe('1. 4. 1942')
    expect(formatDate('1945-05-08')).toBe('8. 5. 1945')
    expect(formatDate(null)).toBe('')
    expect(formatDate('nesmysl')).toBe('')
  })

  it('dateKey normalizuje na yyyy-MM-dd', () => {
    expect(dateKey('1942-04-01T00:00:00')).toBe('1942-04-01')
    expect(dateKey(undefined)).toBeNull()
  })
})

describe('řetězce', () => {
  it('nl2br', () => {
    expect(nl2br('a\nb')).toBe('a<br/>b')
    expect(nl2br(null)).toBe('')
  })

  it('firstLetterToUpper', () => {
    expect(firstLetterToUpper('datum narození')).toBe('Datum narození')
    expect(firstLetterToUpper('')).toBe('')
  })

  it('linkify převede URL a e-mail na odkazy', () => {
    expect(linkify('viz www.memomap.cz/a konec')).toBe(
      'viz <a href="http://www.memomap.cz/a" target="_blank">www.memomap.cz/a</a> konec',
    )
    expect(linkify('napište na info@ehri.cz')).toContain('<a href="mailto:info@ehri.cz">info@ehri.cz</a>')
  })
})

describe('odvozené vlastnosti modelů', () => {
  const address = (from: string | null, to: string | null): AddressInfo => ({
    id: 1, cs: 'Dlouhá 5', en: 'Long 5', de: null, currentCs: null, currentEn: null,
    type: null, typeCs: null, typeEn: null, dateFrom: from, dateTo: to,
  })

  it('addressWithDates – všechny větve', () => {
    expect(addressWithDates(address(null, null), 'cs-CZ')).toBe('Dlouhá 5')
    expect(addressWithDates(address('1939-01-01', '1941-01-01'), 'cs-CZ')).toBe('Dlouhá 5 (1939-1941)')
    expect(addressWithDates(address('1939-01-01', '1939-05-01'), 'cs-CZ')).toBe('Dlouhá 5 (1939)')
    expect(addressWithDates(address(null, '1941-01-01'), 'cs-CZ')).toBe('Dlouhá 5 (do 1941)')
    expect(addressWithDates(address('1939-01-01', null), 'en-US')).toBe('Long 5 (after 1939)')
  })

  it('documentTitle spojí neprázdné části', () => {
    const doc = { labelCs: 'Dopis', descriptionCs: 'popis', creationDateCs: null, creationPlaceCs: 'Praha', ownerCs: '' } as Document
    expect(documentTitle(doc, 'cs-CZ')).toBe('Dopis | popis | Praha')
  })

  it('solrDropDownInfo – anglická verze padá zpět na české místo', () => {
    const place = { labelCs: 'A', labelEn: 'B', placeCs: 'Praha', placeEn: null } as SolrPlace
    expect(solrDropDownInfo(place, 'cs-CZ')).toBe('A | Praha')
    expect(solrDropDownInfo(place, 'en-US')).toBe('B | Praha')
  })

  it('popisky obětí', () => {
    const victim = { label: 'Jan Novák (*1. 1. 1900)', name: 'Jan Novák', birthdate: '1. 1. 1900' } as VictimShortInfoModel
    expect(victimLabelForPicture(victim)).toBe('<b>Jan Novák</b><br/>*1. 1. 1900')
    expect(victimShortLabelForPicture(victim)).toBe('<b>Jan Novák</b><br/>*1. 1. 1900')
    expect(victimLabelForPicture({ ...victim, label: null })).toBe('Jan Novák (*1. 1. 1900)')
    expect(victimBirthdateTitle(victim)).toBe('* 1. 1. 1900')
  })

  it('addressStatistics z pražského domu', () => {
    const house = {
      address: null,
      victims: null,
      pragueAddress: { present19420101: 7, deported19420101: 7, present19450508: 0, deported19450508: 14 },
    } as unknown as AddressWithVictims
    const stats = addressStatistics(house)
    expect(stats).toHaveLength(15)
    expect(stats[0]).toEqual({ date: '1942-01-01', numberPresent: 7, numberAbsent: 7 })
    expect(stats[14]).toEqual({ date: '1945-05-08', numberPresent: 0, numberAbsent: 14 })
    expect(stats[1].numberPresent).toBeNull()
    expect(addressStatistics({ address: null, victims: null, pragueAddress: null })).toEqual([])
  })
})
