// Konfigurace aplikace z public/appsettings.json (stejný soubor jako v Blazor klientovi, sekce "App").
// V režimu vývoje se navíc přimíchá appsettings.Development.json (pokud obsahuje sekci "App").

import { parseLenientJson } from './utils/json'

export type AppState = 'Development' | 'Production' | 'Shutdown'

export interface AppConfig {
  apiUrl: string
  appUrl: string
  appState: AppState
  messageCzech: string
  messageEnglish: string
  cities: string[]
}

interface AppSettingsSection {
  ApiURL?: string
  AppURL?: string
  AppState?: string
  MessageCzech?: string
  MessageEnglish?: string
  Cities?: string[]
}

let config: AppConfig | null = null

async function fetchAppSection(file: string): Promise<AppSettingsSection | null> {
  try {
    const response = await fetch(`${import.meta.env.BASE_URL}${file}?v=${Date.now()}`)
    if (!response.ok) return null
    const json = parseLenientJson<{ App?: AppSettingsSection }>(await response.text())
    return (json?.App as AppSettingsSection) ?? null
  } catch {
    return null
  }
}

function toAppState(value: string | undefined): AppState {
  // stejná logika jako MapService konstruktor: vše kromě Development/Shutdown je Production
  if (value === 'Development') return 'Development'
  if (value === 'Shutdown') return 'Shutdown'
  return 'Production'
}

function ensureTrailingSlash(url: string): string {
  return url && !url.endsWith('/') ? url + '/' : url
}

export async function loadConfig(): Promise<AppConfig> {
  const base = (await fetchAppSection('appsettings.json')) ?? {}
  const dev = import.meta.env.DEV ? await fetchAppSection('appsettings.Development.json') : null
  const merged: AppSettingsSection = { ...base, ...(dev ?? {}) }

  config = {
    apiUrl: ensureTrailingSlash(merged.ApiURL ?? ''),
    appUrl: ensureTrailingSlash(merged.AppURL ?? ''),
    appState: toAppState(merged.AppState),
    messageCzech: merged.MessageCzech ?? '',
    messageEnglish: merged.MessageEnglish ?? '',
    cities: merged.Cities ?? [],
  }
  return config
}

export function getConfig(): AppConfig {
  if (!config) throw new Error('Konfigurace ještě není načtená – volej loadConfig() před startem aplikace.')
  return config
}
