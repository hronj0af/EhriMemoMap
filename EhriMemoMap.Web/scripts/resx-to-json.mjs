// Převede lokalizační .resx soubory Blazor klienta na JSON slovníky pro Vue aplikaci.
// Použití: npm run i18n   (spouští se i automaticky před dev/build)
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const resxDir = resolve(root, '../EhriMemoMap.Client/Resources');
const outDir = resolve(root, 'src/i18n');

const files = {
    cs: 'CommonResources.cs.resx',
    en: 'CommonResources.en.resx',
};

function decodeXml(s) {
    return s
        .replace(/&lt;/g, '<')
        .replace(/&gt;/g, '>')
        .replace(/&quot;/g, '"')
        .replace(/&apos;/g, "'")
        .replace(/&#(\d+);/g, (_, n) => String.fromCodePoint(Number(n)))
        .replace(/&#x([0-9a-f]+);/gi, (_, n) => String.fromCodePoint(parseInt(n, 16)))
        .replace(/&amp;/g, '&');
}

function parseResx(xml) {
    // odstranit XML komentáře (hlavička resx obsahuje ukázkové <data> elementy)
    const body = xml.replace(/<!--[\s\S]*?-->/g, '');
    const result = {};
    const re = /<data\s+name="([^"]+)"[^>]*>\s*<value>([\s\S]*?)<\/value>/g;
    let m;
    while ((m = re.exec(body)) !== null) {
        result[decodeXml(m[1])] = decodeXml(m[2]);
    }
    // a prázdné hodnoty <value />
    const reEmpty = /<data\s+name="([^"]+)"[^>]*>\s*<value\s*\/>/g;
    while ((m = reEmpty.exec(body)) !== null) {
        result[decodeXml(m[1])] = '';
    }
    return result;
}

mkdirSync(outDir, { recursive: true });
const dictionaries = {};
for (const [lang, file] of Object.entries(files)) {
    const xml = readFileSync(resolve(resxDir, file), 'utf8');
    dictionaries[lang] = parseResx(xml);
    const sorted = Object.fromEntries(Object.entries(dictionaries[lang]).sort(([a], [b]) => a.localeCompare(b)));
    writeFileSync(resolve(outDir, `${lang}.json`), JSON.stringify(sorted, null, 2) + '\n', 'utf8');
    console.log(`[resx-to-json] ${lang}: ${Object.keys(sorted).length} klíčů`);
}

// upozornit na klíče, které chybí v jednom z jazyků
const cs = Object.keys(dictionaries.cs);
const en = Object.keys(dictionaries.en);
const missingEn = cs.filter(k => !(k in dictionaries.en));
const missingCs = en.filter(k => !(k in dictionaries.cs));
if (missingEn.length) console.warn(`[resx-to-json] Chybí v en: ${missingEn.join(', ')}`);
if (missingCs.length) console.warn(`[resx-to-json] Chybí v cs: ${missingCs.join(', ')}`);
