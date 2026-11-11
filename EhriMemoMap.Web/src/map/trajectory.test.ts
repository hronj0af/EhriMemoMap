import { describe, expect, it } from 'vitest'
import type { NarrativeMap, NarrativeMapStop, Place } from '../models/api'
import { TRAJECTORY_COLORS, toTrajectoryStops, trajectoryStopColor, trajectoryStopKind, trajectoryStopTooltip } from './trajectory'

function place(partial: Partial<Place>): Place {
  return {
    id: 1,
    type: 'main point',
    labelCs: null,
    labelEn: null,
    addressCs: null,
    addressEn: null,
    townCs: null,
    townEn: null,
    streetCs: null,
    streetEn: null,
    houseNr: null,
    remarkCs: null,
    remarkEn: null,
    mapPoint: null,
    stopId: null,
    narrativeMapId: null,
    documents: null,
    ...partial,
  }
}

function stop(partial: Partial<NarrativeMapStop>, placePartial: Partial<Place> = {}): NarrativeMapStop {
  return {
    id: 1,
    stopOrder: 1,
    labelCs: null,
    labelEn: null,
    dateCs: null,
    dateEn: null,
    descriptionCs: null,
    descriptionEn: null,
    descriptionSectionsCs: null,
    descriptionSectionsEn: null,
    documents: null,
    places: [place(placePartial)],
    ...partial,
  }
}

const HOME = 'Velké Meziříčí'

describe('trajectoryStopKind', () => {
  it.each([
    ['Místo narození', 'birth'],
    ['Bydliště', 'residence'],
    ['Poslední bydliště', 'residence'],
    ['Poslední bydliště před deportací', 'residence'],
    ['Místo věznění', 'imprisonment'],
    ['Místo úmrtí', 'death'],
    ['Místo osvobození', 'liberation'],
    ['Něco jiného', 'other'],
    [null, 'other'],
  ])('%s → %s', (label, kind) => expect(trajectoryStopKind(label)).toBe(kind))
})

describe('trajectoryStopColor', () => {
  it('bydliště v domovském městě vínově, jinde modře', () => {
    expect(trajectoryStopColor(stop({ labelCs: 'Bydliště' }, { townCs: HOME }), HOME)).toBe(TRAJECTORY_COLORS.residenceHome)
    expect(trajectoryStopColor(stop({ labelCs: 'Poslední bydliště před deportací' }, { townCs: 'Brno' }), HOME)).toBe(
      TRAJECTORY_COLORS.residenceElsewhere,
    )
  })

  it('místo úmrtí podle druhu místa', () => {
    expect(trajectoryStopColor(stop({ labelCs: 'Místo úmrtí' }, { category: 'imprisonment_place' }), HOME)).toBe(
      TRAJECTORY_COLORS.deathImprisonment,
    )
    expect(trajectoryStopColor(stop({ labelCs: 'Místo úmrtí' }, { category: 'town' }), HOME)).toBe(TRAJECTORY_COLORS.death)
  })

  it('narození, věznění a osvobození', () => {
    expect(trajectoryStopColor(stop({ labelCs: 'Místo narození' }), HOME)).toBe(TRAJECTORY_COLORS.birth)
    expect(trajectoryStopColor(stop({ labelCs: 'Místo věznění' }), HOME)).toBe(TRAJECTORY_COLORS.imprisonment)
    expect(trajectoryStopColor(stop({ labelCs: 'Místo osvobození' }), HOME)).toBe(TRAJECTORY_COLORS.liberation)
  })
})

describe('trajectoryStopTooltip', () => {
  const s = stop(
    { labelCs: 'Bydliště', labelEn: 'Residence', dateCs: '25. 9. 1938 – 3. 10. 1938', dateEn: '25 Sep 1938 – 3 Oct 1938' },
    { addressCs: 'Velké Meziříčí, Dalimilova 41', addressEn: 'Velké Meziříčí, Dalimilova 41' },
  )

  it('tři řádky: název tučně, místo, datace', () => {
    expect(trajectoryStopTooltip(s, false)).toBe('<b>Bydliště</b><br/>Velké Meziříčí, Dalimilova 41<br/>25. 9. 1938 – 3. 10. 1938')
    expect(trajectoryStopTooltip(s, true)).toBe('<b>Residence</b><br/>Velké Meziříčí, Dalimilova 41<br/>25 Sep 1938 – 3 Oct 1938')
  })

  it('chybějící údaje vynechá a HTML z dat escapuje', () => {
    expect(trajectoryStopTooltip(stop({ labelCs: 'Místo <b>věznění</b>' }, { addressCs: 'Minsk' }), false)).toBe(
      '<b>Místo &lt;b&gt;věznění&lt;/b&gt;</b><br/>Minsk',
    )
  })
})

describe('toTrajectoryStops', () => {
  it('řadí podle stop_order a vynechá zastavení bez souřadnic', () => {
    const trajectory: NarrativeMap = {
      id: 1,
      type: 6,
      labelCs: null,
      labelEn: null,
      descriptionCs: null,
      descriptionEn: null,
      mainPoints: null,
      stops: [
        stop({ id: 3, stopOrder: 3, labelCs: 'Místo věznění' }, { mapPoint: '{"type":"Point","coordinates":[27.5,53.9]}' }),
        stop({ id: 1, stopOrder: 1, labelCs: 'Místo narození' }, { mapPoint: '{"type":"Point","coordinates":[16.6,49.2]}' }),
        stop({ id: 2, stopOrder: 2, labelCs: 'Bydliště' }, { mapPoint: null }),
      ],
    }
    const result = toTrajectoryStops(trajectory, HOME, false)
    expect(result.map((a) => [a.lat, a.lng, a.color])).toEqual([
      [49.2, 16.6, TRAJECTORY_COLORS.birth],
      [53.9, 27.5, TRAJECTORY_COLORS.imprisonment],
    ])
  })
})
