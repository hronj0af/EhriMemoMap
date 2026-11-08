// Jednoduchá lokalizace – náhrada IStringLocalizer<CommonResources>.
// Slovníky cs.json/en.json se generují z .resx (scripts/resx-to-json.mjs).
// Texty nemají parametry, část obsahuje HTML (v šablonách přes v-html).
import { computed, ref } from 'vue'
import cs from './cs.json'
import en from './en.json'

export type Culture = 'cs-CZ' | 'en-US'
export type Lang = 'cs' | 'en'

/** Klíč v localStorage – stejný jako v Blazor verzi, aby uživatelům zůstala zvolená řeč. */
const STORAGE_KEY = 'BlazorCulture'
const DEFAULT_CULTURE: Culture = 'cs-CZ'

const dictionaries: Record<Lang, Record<string, string>> = { cs, en }

function readStoredCulture(): Culture | null {
  try {
    const value = window.localStorage.getItem(STORAGE_KEY)
    return value === 'cs-CZ' || value === 'en-US' ? value : null
  } catch {
    return null
  }
}

function storeCulture(value: Culture): void {
  try {
    window.localStorage.setItem(STORAGE_KEY, value)
  } catch {
    // localStorage nemusí být dostupné (anonymní režim) – volba pak platí jen do reloadu
  }
}

const stored = readStoredCulture()
if (stored === null) storeCulture(DEFAULT_CULTURE)

const culture = ref<Culture>(stored ?? DEFAULT_CULTURE)
const lang = computed<Lang>(() => (culture.value === 'en-US' ? 'en' : 'cs'))

/** Přeložený text; chybějící klíč vrací samotný klíč (stejně jako StringLocalizer). */
export function t(key: string): string {
  return dictionaries[lang.value][key] ?? key
}

/** Vrátí českou nebo anglickou variantu podle aktuálního jazyka (náhrada CultureInfo.CurrentCulture.Name == "en-US" ? En : Cs). */
export function byLang<T>(csValue: T, enValue: T): T {
  return lang.value === 'en' ? enValue : csValue
}

export function getCulture(): Culture {
  return culture.value
}

/**
 * Změna jazyka. Stejně jako Blazor verze stránku znovu načte – texty z API (popisky míst)
 * se počítají při načtení dat, reload je nejjednodušší způsob, jak mít vše konzistentní.
 */
export function setCulture(value: Culture, reload = true): void {
  storeCulture(value)
  culture.value = value
  document.documentElement.lang = value === 'en-US' ? 'en' : 'cs'
  if (reload) window.location.reload()
}

export function useI18n() {
  return { t, byLang, culture, lang, setCulture }
}

document.documentElement.lang = lang.value
