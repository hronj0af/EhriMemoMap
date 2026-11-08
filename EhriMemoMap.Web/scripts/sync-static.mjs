// Synchronizuje statické soubory ze stávajícího Blazor klienta (EhriMemoMap.Client/wwwroot) do public/.
// Během migrace tak soubory (fonty, obrázky, PDF metodik – desítky MB) nejsou v repu dvakrát.
// Synchronizované cesty jsou v .gitignore. Po odstranění Blazor klienta se přesunou do public/ natrvalo (git mv).
import { cpSync, existsSync, mkdirSync, statSync, readdirSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const source = resolve(root, '../EhriMemoMap.Client/wwwroot');
const target = resolve(root, 'public');

// Položky, které se kopírují 1:1 (soubory i složky)
// Styly a písma jsou v bundlu (src/styles) – synchronizují se jen obrázky, na které se odkazuje cestou css/images/…
// (ikony markerů). Složky fonts/ (lokální Barlow, Noto Serif – nepoužívané) a radzen-custom/ se nepřenášejí.
export const SYNCED = [
    'appsettings.json',
    'appsettings.Development.json',
    'favicon.png',
    'css/images',
    'images',
    'metodiky',
];

if (!existsSync(source)) {
    console.error(`[sync-static] Zdrojová složka neexistuje: ${source}`);
    process.exit(1);
}

mkdirSync(target, { recursive: true });

const items = [
    ...SYNCED,
    // všechny mapsettings.{city}.json
    ...readdirSync(source).filter(f => /^mapsettings\..+\.json$/.test(f)),
];

let copied = 0;
for (const item of items) {
    const from = join(source, item);
    if (!existsSync(from)) {
        console.warn(`[sync-static] Chybí: ${item}`);
        continue;
    }
    // kopíruje jen novější soubory (rychlé při opakovaném spuštění)
    cpSync(from, join(target, item), {
        recursive: true,
        force: true,
        filter: (src) => {
            const dest = join(target, src.substring(source.length));
            if (!existsSync(dest)) return true;
            const s = statSync(src);
            if (s.isDirectory()) return true;
            return s.mtimeMs > statSync(dest).mtimeMs || s.size !== statSync(dest).size;
        },
    });
    copied++;
}
console.log(`[sync-static] Synchronizováno ${copied} položek do public/`);
