// Jednorázový převod: vytáhne vložené SVG ikony z Razor komponent Blazor klienta do Vue komponent
// src/components/icons/*.vue (beze změny obsahu SVG). Spouští se ručně: node scripts/extract-icons.mjs
// Razor výrazy v atributu style (`style="@(Disabled ? ... : "")"`) se odstraní – styl dodá Vue komponenta.
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs'
import { dirname, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..')
const client = resolve(root, '../EhriMemoMap.Client')
const outDir = resolve(root, 'src/components/icons')

// [soubor, pořadí SVG v souboru (0-based), název Vue komponenty]
const ICONS = [
  ['Components/Buttons/HelpButton.razor', 0, 'IconHelp'],
  ['Components/Buttons/HelpButton.razor', 1, 'IconHelpMobile'],
  ['Components/Buttons/HomeButton.razor', 0, 'IconHome'],
  ['Components/Buttons/JoinButton.razor', 0, 'IconJoin'], // obsahuje vnořené <svg>
  ['Components/Buttons/JoinButton.razor', 1, 'IconJoinMobile'],
  ['Components/Buttons/LanguageButton.razor', 0, 'IconLanguageBackground'],
  ['Components/Buttons/LayersButton.razor', 0, 'IconLayers'],
  ['Components/Buttons/LayersButton.razor', 1, 'IconLayersMobile'],
  ['Components/Buttons/SearchButton.razor', 0, 'IconSearch'],
  ['Components/Buttons/SearchButton.razor', 1, 'IconSearchMobile'],
  ['Components/Buttons/StoryMapButton.razor', 0, 'IconStoryMap'],
  ['Components/Buttons/StoryMapButton.razor', 1, 'IconStoryMapMobile'],
  ['Components/Buttons/TimelineButton.razor', 0, 'IconTimeline'],
  ['Components/Buttons/TimelineButton.razor', 1, 'IconTimelineMobile'],
  ['Components/ButtonWrappers/ZoomButtonsWrapper.razor', 0, 'IconMyLocation'],
  ['Components/ButtonWrappers/ZoomButtonsWrapper.razor', 1, 'IconZoomIn'],
  ['Components/ButtonWrappers/ZoomButtonsWrapper.razor', 2, 'IconZoomOut'],
  ['Components/Dialogs/Search/Search.razor', 0, 'IconHelpSmall'],
]

/** Najde všechny SVG bloky nejvyšší úrovně (s ohledem na vnořené <svg>). */
function topLevelSvgs(text) {
  const result = []
  const re = /<svg\b|<\/svg>/g
  let depth = 0
  let start = -1
  let m
  while ((m = re.exec(text)) !== null) {
    if (m[0] === '</svg>') {
      depth--
      if (depth === 0) result.push(text.substring(start, m.index + 6))
    } else {
      if (depth === 0) start = m.index
      depth++
    }
  }
  return result
}

mkdirSync(outDir, { recursive: true })
const cache = new Map()
for (const [file, index, name] of ICONS) {
  if (!cache.has(file)) cache.set(file, topLevelSvgs(readFileSync(resolve(client, file), 'utf8')))
  let svg = cache.get(file)[index]
  if (!svg) throw new Error(`${file}: SVG #${index} nenalezeno`)
  svg = svg.replace(/ style="@\(.*?\)"/g, '')
  if (svg.includes('@')) throw new Error(`${file} #${index}: zbyl Razor výraz`)
  const content = `<!-- Vygenerováno skriptem scripts/extract-icons.mjs z ${file} (SVG #${index}) -->\n<template>\n  ${svg.trim()}\n</template>\n`
  writeFileSync(resolve(outDir, `${name}.vue`), content, 'utf8')
  console.log(`${name}.vue`)
}
