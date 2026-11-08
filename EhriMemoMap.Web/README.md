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

Legacy bundle lze vyzkoušet i v moderním prohlížeči: v `dist/index.html` odstranit skripty `type="module"`
a u zbylých atribut `nomodule` – aplikace se pak spustí jen z `*-legacy-*.js` (SystemJS).

## Nasazení

1. `npm ci && npm run build` (v repozitáři – build potřebuje `../EhriMemoMap.Client` kvůli statickým souborům
   a slovníkům, dokud není Blazor klient odstraněný).
2. Obsah `dist/` nahrát na web (celý adresář nahradit – staré soubory v `assets/` lze smazat).
3. Webový server musí cesty aplikace (`/prague`, `/pacov`…) obsloužit souborem `index.html` a nesmí
   cachovat `index.html` ani `*settings*.json`. Ukázky:
   - **nginx** (memomap.cz): `deploy/nginx/memomap.conf`
   - **IIS**: `deploy/iis/web.config` (zkopírovat do kořene webu; vyžaduje modul URL Rewrite)

`dist/` obsahuje i zdrojové mapy (`*.js.map`) pro ladění v prohlížeči; pokud nemají být veřejné, nenahrávat je.

### Přepnutí z Blazoru

1. Nasadit na testovací adresu (např. subdoména) – API povoluje požadavky z libovolné domény (CORS).
2. Otestovat na starém telefonu, kde Blazor nešel spustit, a na současném Androidu a iPhonu (vzdálené ladění:
   Android `chrome://inspect`, iOS Safari → Vývoj). Projít: úvod, mapa, klik na adresu, detail oběti, hledání,
   vrstvy, časová osa, příběhová mapa, galerie, přepnutí jazyka.
3. Přepnout produkci – obsah webu nahradit `dist/` (původní publikaci Blazoru si ponechat pro návrat),
   v nginx doplnit pravidla z `deploy/nginx/memomap.conf`.

### Odstranění Blazor klienta (po ověření v provozu)

1. Statické soubory natrvalo do `public/`: `git mv` z `EhriMemoMap.Client/wwwroot` – `images/`, `css/images/`,
   `metodiky/`, `favicon.png`, `appsettings*.json`, `mapsettings.*.json`.
2. Slovníky: vygenerované `src/i18n/cs.json` a `en.json` commitnout a dál upravovat přímo (texty jsou HTML/prostý text).
3. Ze `package.json` odebrat `prepare-assets`/`predev`/`prebuild`, smazat `scripts/sync-static.mjs` a
   `scripts/resx-to-json.mjs`, z `.gitignore` odebrat synchronizované cesty.
4. `scripts/purge-radzen-css.mjs` čte téma z Blazor klienta – buď `material-base.css` přesunout do
   `EhriMemoMap.Web/scripts/`, nebo dál upravovat přímo `src/styles/radzen.css` a skript smazat.
5. Odebrat `EhriMemoMap.Client` z řešení (`EhriMemoMap.sln`) a smazat.
