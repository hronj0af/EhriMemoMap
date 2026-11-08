/// <reference types="vitest/config" />
import vue from '@vitejs/plugin-vue'
import legacy from '@vitejs/plugin-legacy'
import { defineConfig } from 'vite'

// Podporované prohlížeče: moderní bundle pro současné prohlížeče, legacy bundle (Babel + core-js polyfilly)
// pro starší mobily. Vue 3 vyžaduje Proxy, proto níž než Safari 10 / Chrome 49 jít nelze.
// Cíle upravit podle statistik prohlížečů návštěvníků v GA4.
const legacyTargets = ['defaults', 'safari >= 11', 'ios_saf >= 11', 'chrome >= 61', 'android >= 61', 'samsung >= 8', 'not IE 11']

// https://vite.dev/config/
export default defineConfig({
  base: '/',
  plugins: [
    // Obrázky jsou statické soubory v public/ (cesty "images/…" jako v Blazoru) – nepřevádět je na importy modulů.
    vue({ template: { transformAssetUrls: false } }),
    // Vytvoří dva bundly: moderní (<script type="module">) a legacy (<script nomodule>, Babel + core-js).
    // Prohlížeč si sám načte ten, kterému rozumí. build.target nastavuje plugin sám.
    legacy({
      targets: legacyTargets,
    }),
  ],
  build: {
    cssTarget: ['safari11', 'chrome61'],
    // Jeden CSS soubor pro celou aplikaci. Při rozdělení po chunkcích by legacy build (SystemJS)
    // vkládal styly ještě jednou do JavaScriptu (+26 kB pro staré prohlížeče).
    cssCodeSplit: false,
    sourcemap: true,
  },
  server: {
    port: 5173,
  },
  test: {
    environment: 'happy-dom',
    include: ['src/**/*.test.ts'],
  },
})
