// Vygeneruje src/styles/radzen.css – z Radzen tématu (EhriMemoMap.Client/wwwroot/radzen-custom/css/material-base.css,
// ~24 000 řádků) ponechá jen pravidla, která aplikace používá. Spouští se ručně po změně komponent nebo tématu:
//   node scripts/purge-radzen-css.mjs
//
// Třídy se hledají ve zdrojích (src/**/*.vue|ts, index.html, slovníky i18n). Třídy skládané v kódu
// (např. `rz-col-${size}`, `rz-variant-${variant}`) pokrývá DYNAMIC_CLASSES – při použití nové hodnoty
// (např. jiného buttonStyle) nebo nové UI komponenty s dynamickými třídami je potřeba seznam doplnit.
import { copyFileSync, mkdirSync, readdirSync, readFileSync, writeFileSync } from 'node:fs'
import { dirname, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { PurgeCSS } from 'purgecss'

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..')
/** PurgeCSS (glob) vyžaduje cesty s dopřednými lomítky i na Windows */
const glob = (path) => path.split('\\').join('/')
const radzenDir = resolve(root, '../EhriMemoMap.Client/wwwroot/radzen-custom')
const outDir = resolve(root, 'src/styles')

const VAR_DEFINITION = /(--[\w-]+)\s*:/g
const VAR_USAGE = /var\(\s*(--[\w-]+)/g

// HTML značky, které se mohou objevit v obsahu z API nebo ze slovníků (v-html)
const TAGS = [
  'html', 'body', 'main', 'aside', 'div', 'span', 'p', 'a', 'b', 'i', 'em', 'strong', 'small', 'br', 'hr',
  'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'ul', 'ol', 'li', 'table', 'thead', 'tbody', 'tr', 'td', 'th',
  'img', 'input', 'button', 'label', 'svg', 'iframe', 'video', 'audio',
]

// Třídy skládané v kódu UI komponent (src/components/ui) – jen hodnoty, které aplikace používá.
// Vzory jsou "standard": selektor se ponechá, jen pokud jsou přítomné všechny jeho třídy
// (např. .rz-button.rz-variant-flat.rz-light ano, .rz-alert.rz-variant-flat ne).
const DYNAMIC_CLASSES = [
  /^rz-col(-(sm|md|lg))?-\d+$/, // RzColumn
  /^rz-offset-\d+$/, // RzColumn offset
  /^rz-align-items-(normal|center|flex-start|flex-end)$/, // RzRow/RzStack
  /^rz-justify-content-(normal|center|flex-start|flex-end|left|space-between|space-around)$/,
  /^rz-flex-(row|column)$/, // RzStack
  /^rz-button-(xs|sm|md)$/, // RzButton size
  /^rz-variant-(filled|flat|outlined|text)$/, // RzButton variant
  /^rz-(primary|secondary|light|dark)$/, // RzButton buttonStyle
  /^rz-shade-default$/,
  /^rz-state-disabled$/,
  /^rz-dialog-side-position-(right|bottom)$/, // SideDialog
]

// CSS proměnné použité mimo Radzen téma (ostatní CSS, inline styly a skripty) se musí ponechat,
// i když je ponechaná pravidla tématu nepoužívají – např. var(--Wine-Red) v šablonách.
function listFiles(dir, ext) {
  const out = []
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const full = resolve(dir, entry.name)
    if (entry.isDirectory()) out.push(...listFiles(full, ext))
    else if (ext.some((e) => entry.name.endsWith(e)) && entry.name !== 'radzen.css') out.push(full)
  }
  return out
}
const externalSources = [...listFiles(resolve(root, 'src'), ['.vue', '.ts', '.css']), resolve(root, 'index.html')]
const keepVariables = new Set()
for (const file of externalSources) {
  for (const m of readFileSync(file, 'utf8').matchAll(VAR_USAGE)) keepVariables.add(m[1])
}

const original = readFileSync(resolve(radzenDir, 'css/material-base.css'), 'utf8')
const definedInOriginal = new Set([...original.matchAll(VAR_DEFINITION)].map((m) => m[1]))

/** Proměnné použité v CSS (var(--x)), které v něm nejsou definované, ale v původním tématu ano. */
function missingVariables(css) {
  const defined = new Set([...css.matchAll(VAR_DEFINITION)].map((m) => m[1]))
  const used = new Set([...css.matchAll(VAR_USAGE)].map((m) => m[1]))
  return [...used].filter((v) => !defined.has(v) && definedInOriginal.has(v))
}

// PurgeCSS občas odstraní proměnnou, na kterou odkazuje jiná ponechaná proměnná – chybějící se doplní
// do safelistu a čištění se opakuje, dokud nejsou všechny použité proměnné definované.
let css = ''
for (let attempt = 0; attempt < 10; attempt++) {
  const result = await new PurgeCSS().purge({
    content: [
      glob(resolve(root, 'index.html')),
      glob(resolve(root, 'src/**/*.vue')),
      glob(resolve(root, 'src/**/*.ts')),
      glob(resolve(root, 'src/i18n/*.json')),
    ],
    css: [glob(resolve(radzenDir, 'css/material-base.css'))],
    // @font-face se ponechají všechny; CSS proměnné jen ty, které se někde používají
    fontFace: false,
    variables: true,
    keyframes: true,
    safelist: {
      standard: [...TAGS, ...DYNAMIC_CLASSES],
      variables: [...keepVariables],
    },
  })
  css = result[0].css
  const missing = missingVariables(css)
  if (missing.length === 0) break
  missing.forEach((v) => keepVariables.add(v))
}
if (missingVariables(css).length > 0) throw new Error('Nepodařilo se ponechat všechny použité CSS proměnné')

// fonty, na které se trimované CSS odkazuje (../fonts/X) → src/styles/fonts/X (Vite je zabalí s hashem)
mkdirSync(resolve(outDir, 'fonts'), { recursive: true })
const fonts = new Set([...css.matchAll(/url\(["']?\.\.\/fonts\/([^"')]+)["']?\)/g)].map((m) => m[1]))
for (const font of fonts) copyFileSync(resolve(radzenDir, 'fonts', font), resolve(outDir, 'fonts', font))
css = css.replace(/url\((["']?)\.\.\/fonts\//g, 'url($1./fonts/')
// @charset musí být na úplném začátku souboru – po spojení s ostatními styly (index.css) tam není
// a PostCSS hlásí varování; soubory jsou UTF-8 i bez něj.
css = css.replace(/^\s*@charset\s+"[^"]*";\s*/gm, '')

const header = `/* Vygenerováno skriptem scripts/purge-radzen-css.mjs z radzen-custom/css/material-base.css
   (jen pravidla používaná aplikací). Neupravovat ručně – změny dělat ve zdroji a skript spustit znovu. */\n`
writeFileSync(resolve(outDir, 'radzen.css'), header + css, 'utf8')

console.log(`radzen.css: ${(original.length / 1024).toFixed(0)} kB → ${(css.length / 1024).toFixed(0)} kB, fonty: ${[...fonts].join(', ')}`)
