# EhriMemoMap.Web

Vue 3 + Vite + TypeScript verze klienta MemoMap (náhrada Blazor WASM projektu `EhriMemoMap.Client`).
Komunikuje se stejným API (`EhriMemoMap.API`).

## Spuštění

```bash
npm install
npm run dev        # http://localhost:5173
npm run build      # produkční build do dist/ (moderní + legacy bundle)
npm run typecheck
npm test           # jednotkové testy (vitest)
```

`dev` i `build` nejdřív spustí `prepare-assets`:

- `scripts/resx-to-json.mjs` vygeneruje `src/i18n/cs.json` a `en.json` z `EhriMemoMap.Client/Resources/*.resx`
  (texty se zatím dál upravují v `.resx`).
- `scripts/sync-static.mjs` zkopíruje statické soubory (obrázky, fonty, CSS, metodiky, `mapsettings.*.json`,
  `appsettings*.json`) z `EhriMemoMap.Client/wwwroot` do `public/`.

Obojí je v `.gitignore`, ať nejsou soubory v repu dvakrát. Po odstranění Blazor klienta se přesunou natrvalo.

## Struktura

- `src/stores/map.ts` – stav a logika mapy (přepis `MapService*.cs`)
- `src/map/` – Leaflet (`mapApi.ts` = přepis `site.ts`, úpravy Leafletu, pluginy v `vendor/`)
- `src/components/` – komponenty zrcadlí `EhriMemoMap.Client/Components` (buttons, buttonWrappers, dialogs)
- `src/components/ui/` – náhrady Radzen komponent; generují stejné HTML třídy, vzhled dodává
  `radzen-custom/css/material-base.css`
- `src/components/icons/` – SVG ikony vytažené z Razor komponent skriptem `scripts/extract-icons.mjs`
  (jednorázově; ikony se dál upravují přímo ve `.vue` souborech)

## Styly

Všechny styly jsou v `src/styles/` a do aplikace se dostávají přes `<link>` v `index.html` (Vite z nich udělá
jeden soubor s hashem; `postcss-preset-env` doplní fallbacky pro starší prohlížeče):

- `site.css`, `app.css`, `leaflet.css`, `bigger-picture.css`, `video-js.css` – převzaté z Blazor klienta,
  dál se upravují přímo tady.
- `radzen.css` – **generovaný** výběr z Radzen tématu (`radzen-custom/css/material-base.css`, ~980 kB → ~44 kB)
  jen s pravidly, která aplikace používá. Po přidání nových tříd Radzenu do komponent (nebo nové hodnoty
  `buttonStyle`, `variant`…) je potřeba ho přegenerovat: `node scripts/purge-radzen-css.mjs`
  (dynamicky skládané třídy jsou vyjmenované v `DYNAMIC_CLASSES` ve skriptu).

Písma Barlow a Noto Serif se načítají z Google Fonts, ikony tlačítek (Material Icons) jsou v `src/styles/fonts/`.

## Konfigurace

`public/appsettings.json`, sekce `App` (stejná jako v Blazoru): `ApiURL`, `AppState` (`Shutdown` zobrazí chybovou
hlášku `MessageCzech/MessageEnglish`), `Cities` (města na úvodní stránce).
V `npm run dev` se navíc přimíchá `appsettings.Development.json`, pokud obsahuje sekci `App`.

`mapsettings.{city}.json` se načítá z vlastního originu aplikace (Blazor používal `AppURL`).

## Podpora prohlížečů

`@vitejs/plugin-legacy` vytváří legacy bundle (Babel + core-js) pro starší prohlížeče, cíle jsou ve `vite.config.ts`
(`legacyTargets`) a `postcss.config.js`. Minimum dané Vue 3 je Safari 10 / Chrome 49.

## Nasazení (IIS)

Obsah `dist/` na web. `public/web.config` zajišťuje SPA fallback na `index.html` (cesty `/prague`, `/ricany`…)
a vypíná cache pro `index.html`.
