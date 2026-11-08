// Tolerantní parsování JSON konfiguračních souborů (appsettings.json, mapsettings.*.json).
// Blazor je četl přes Microsoft.Extensions.Configuration / Newtonsoft, které tolerují komentáře
// a koncové čárky – soubory je skutečně obsahují (např. zakomentované "url" v mapsettings.pacov.json).

/**
 * Projde text mimo řetězce a pro každý znak zavolá handler, který vrátí, kolik znaků zkonzumoval
 * a co zapsat do výstupu. Řetězce (včetně escape sekvencí) se kopírují beze změny.
 */
function transformOutsideStrings(text: string, handler: (i: number) => { skip: number; out: string } | null): string {
  let result = ''
  let i = 0
  while (i < text.length) {
    const ch = text[i]
    if (ch === '"') {
      let j = i + 1
      while (j < text.length && text[j] !== '"') j += text[j] === '\\' ? 2 : 1
      result += text.substring(i, j + 1)
      i = j + 1
      continue
    }
    const handled = handler(i)
    if (handled) {
      result += handled.out
      i += handled.skip
    } else {
      result += ch
      i++
    }
  }
  return result
}

/** Odstraní // a /* *\/ komentáře. */
export function stripJsonComments(text: string): string {
  return transformOutsideStrings(text, (i) => {
    if (text[i] !== '/') return null
    if (text[i + 1] === '/') {
      const end = text.indexOf('\n', i)
      return { skip: (end === -1 ? text.length : end) - i, out: '' }
    }
    if (text[i + 1] === '*') {
      const end = text.indexOf('*/', i + 2)
      return { skip: (end === -1 ? text.length : end + 2) - i, out: '' }
    }
    return null
  })
}

/** Odstraní koncové čárky před ] a }. */
export function stripTrailingCommas(text: string): string {
  return transformOutsideStrings(text, (i) => {
    if (text[i] !== ',') return null
    let j = i + 1
    while (j < text.length && /\s/.test(text[j])) j++
    return text[j] === ']' || text[j] === '}' ? { skip: 1, out: '' } : null
  })
}

/** JSON.parse s tolerancí BOM, komentářů a koncových čárek. */
export function parseLenientJson<T = unknown>(text: string): T {
  return JSON.parse(stripTrailingCommas(stripJsonComments(text.replace(/^﻿/, '')))) as T
}
