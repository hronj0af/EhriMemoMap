import { describe, expect, it } from 'vitest'
import { parseLenientJson } from './json'

describe('parseLenientJson', () => {
  it('komentáře, koncové čárky a BOM', () => {
    const text = `﻿{
      // komentář
      "url": "https://api.memomap.cz/", /* blok */
      //"url": "https://localhost:7294/",
      "list": [1, 2, ],
      "nested": { "a": "x // není komentář", "b": "y, }", },
    }`
    expect(parseLenientJson(text)).toEqual({
      url: 'https://api.memomap.cz/',
      list: [1, 2],
      nested: { a: 'x // není komentář', b: 'y, }' },
    })
  })

  it('escapované uvozovky v řetězci', () => {
    expect(parseLenientJson('{"a": "he said \\"hi\\" // ok"}')).toEqual({ a: 'he said "hi" // ok' })
  })
})
