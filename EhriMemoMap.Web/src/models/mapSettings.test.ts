import { readFileSync, readdirSync } from 'node:fs'
import { resolve } from 'node:path'
import { describe, expect, it } from 'vitest'
import { normalizeMapSettings } from './mapSettings'
import { parseLenientJson } from '../utils/json'

const publicDir = resolve(__dirname, '../../public')

describe('normalizeMapSettings', () => {
  it('převede enumy, čísla a souřadnice', () => {
    const map = normalizeMapSettings({
      map: {
        initialVariables: { city: 'prague', zoom: 12, minBounds: { X: -90, Y: -80 }, storyMaps: true },
        layers: [
          { name: 'OSM', type: 'base', zIndex: '50', selected: true },
          { name: 'inaccessible', type: 'polygons', placeType: 'inaccessible', fontColor: 'Dark', minZoom: 14 },
          { name: 'address', type: 'objects', placeType: 'addressLastResidence', quickAccess: true },
        ],
        timeline: [{ name: '1942_Q1', from: '1942-01-01', to: '1942-03-31' }, { name: 'all' }],
      },
    })!
    expect(map.initialVariables?.minBounds).toEqual({ x: -90, y: -80 })
    expect(map.initialVariables?.storyMaps).toBe(true)
    expect(map.layers?.[0]).toMatchObject({ type: 'Base', zIndex: 50, selected: true, placeType: null })
    expect(map.layers?.[1]).toMatchObject({ type: 'Polygons', placeType: 'Inaccessible', fontColor: 'Dark', minZoom: 14 })
    expect(map.layers?.[2]).toMatchObject({ type: 'Objects', placeType: 'AddressLastResidence', quickAccess: true })
    expect(map.timeline?.[1]).toMatchObject({ name: 'all', from: null, to: null, selected: false, additionalLayers: null })
  })

  it('všechny mapsettings.*.json se načtou a mají známé typy vrstev', () => {
    const files = readdirSync(publicDir).filter((f) => /^mapsettings\..+\.json$/.test(f))
    expect(files.length).toBeGreaterThan(0)
    for (const file of files) {
      const map = normalizeMapSettings(parseLenientJson(readFileSync(resolve(publicDir, file), 'utf8')))
      expect(map, file).not.toBeNull()
      expect(map!.initialVariables?.city, file).toBeTruthy()
      for (const layer of map!.layers ?? []) {
        expect(layer.type, `${file}: ${layer.name}`).not.toBeNull()
      }
    }
  })
})
