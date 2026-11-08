import { describe, expect, it } from 'vitest'
import { resolveRefs, toQuery } from './client'

describe('resolveRefs', () => {
  it('rozbalí pole v $values a odstraní $id', () => {
    const result = resolveRefs<{ id: number }[]>({ $id: '1', $values: [{ $id: '2', id: 5 }, { $id: '3', id: 6 }] })
    expect(result).toEqual([{ id: 5 }, { id: 6 }])
  })

  it('prázdné pole', () => {
    expect(resolveRefs({ $id: '1', $values: [] })).toEqual([])
  })

  it('$ref nahradí stejnou instancí objektu', () => {
    const result = resolveRefs<any>({
      $id: '1',
      a: { $id: '2', x: 1 },
      b: { $ref: '2' },
      list: { $id: '3', $values: [{ $ref: '2' }] },
    })
    expect(result.b).toBe(result.a)
    expect(result.list[0]).toBe(result.a)
  })

  it('cyklický odkaz na kořen', () => {
    const result = resolveRefs<any>({ $id: '1', c: { $id: '2', $values: [{ $ref: '1' }] } })
    expect(result.c[0]).toBe(result)
  })

  it('vnořená pole uvnitř objektů v poli', () => {
    const result = resolveRefs<any>({
      $id: '1',
      $values: [{ $id: '2', documents: { $id: '3', $values: [{ $id: '4', id: 1 }] } }],
    })
    expect(result[0].documents).toEqual([{ id: 1 }])
  })

  it('primitivní hodnoty a null projdou beze změny', () => {
    expect(resolveRefs(null)).toBeNull()
    expect(resolveRefs(5)).toBe(5)
    expect(resolveRefs<any>({ $id: '1', a: null, b: 'x' })).toEqual({ a: null, b: 'x' })
  })
})

describe('toQuery', () => {
  it('vynechá null/undefined a kóduje hodnoty', () => {
    expect(toQuery({ city: 'prague', total: false, timeLinePoint: null, x: undefined, q: 'a b' })).toBe(
      'city=prague&total=false&q=a%20b',
    )
  })
})
