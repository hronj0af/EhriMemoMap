// HTTP klient pro EhriMemoMap.API.
//
// API serializuje s ReferenceHandler.Preserve, takže odpovědi vypadají takto:
//   pole:   {"$id":"1","$values":[ ... ]}
//   objekt: {"$id":"2", ...}  a opakovaný výskyt téhož objektu: {"$ref":"2"}
// Newtonsoft v Blazor klientovi to rozbaloval automaticky; tady to dělá resolveRefs().
import { getConfig } from '../config'

export class ApiError extends Error {
  readonly status: number
  readonly method: string

  constructor(method: string, status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.method = method
  }
}

type Json = null | boolean | number | string | Json[] | { [key: string]: Json }

/**
 * Převede odpověď se $id/$ref/$values na běžná JS data.
 * $ref se nahradí stejnou instancí objektu, na který odkazuje (jako u Newtonsoftu).
 */
export function resolveRefs<T = unknown>(input: unknown): T {
  const byId = new Map<string, unknown>()
  // odkazy, jejichž cíl se objevil až později v dokumentu – doplní se po průchodu
  const pending: { holder: Record<string, unknown> | unknown[]; key: string | number; ref: string }[] = []

  const visit = (node: Json): unknown => {
    if (node === null || typeof node !== 'object') return node

    if (Array.isArray(node)) {
      const arr: unknown[] = []
      node.forEach((item, i) => assign(arr, i, item))
      return arr
    }

    if (typeof node.$ref === 'string') {
      // samostatný $ref mimo kontext assign (kořen) – vrátí známý objekt
      return byId.get(node.$ref) ?? null
    }

    const id = typeof node.$id === 'string' ? node.$id : null

    if (Array.isArray(node.$values)) {
      const arr: unknown[] = []
      if (id) byId.set(id, arr)
      node.$values.forEach((item, i) => assign(arr, i, item))
      return arr
    }

    const obj: Record<string, unknown> = {}
    if (id) byId.set(id, obj)
    for (const key of Object.keys(node)) {
      if (key === '$id') continue
      assign(obj, key, node[key])
    }
    return obj
  }

  const assign = (holder: Record<string, unknown> | unknown[], key: string | number, value: Json) => {
    if (value !== null && typeof value === 'object' && !Array.isArray(value) && typeof value.$ref === 'string') {
      const target = byId.get(value.$ref)
      if (target !== undefined) {
        ;(holder as Record<string | number, unknown>)[key] = target
      } else {
        ;(holder as Record<string | number, unknown>)[key] = null
        pending.push({ holder, key, ref: value.$ref })
      }
      return
    }
    ;(holder as Record<string | number, unknown>)[key] = visit(value)
  }

  const result = visit(input as Json)
  for (const p of pending) {
    ;(p.holder as Record<string | number, unknown>)[p.key] = byId.get(p.ref) ?? null
  }
  return result as T
}

export interface RequestOptions {
  signal?: AbortSignal
}

async function handleResponse<T>(method: string, response: Response): Promise<T> {
  if (!response.ok) {
    throw new ApiError(method, response.status, `API ${method} vrátilo ${response.status} ${response.statusText}`)
  }
  const text = await response.text()
  if (!text) return null as T
  return resolveRefs<T>(JSON.parse(text))
}

/** Sestaví query string; hodnoty null/undefined vynechá. */
export function toQuery(params: Record<string, string | number | boolean | null | undefined>): string {
  const parts: string[] = []
  for (const [key, value] of Object.entries(params)) {
    if (value === null || value === undefined) continue
    parts.push(`${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`)
  }
  return parts.join('&')
}

/** GET {ApiURL}{method}?{params} – náhrada MapService.GetResultFromApiGet. */
export async function apiGet<T>(
  method: string,
  params: Record<string, string | number | boolean | null | undefined> = {},
  options: RequestOptions = {},
): Promise<T> {
  const query = toQuery(params)
  const url = getConfig().apiUrl + method + (query ? '?' + query : '')
  const response = await fetch(url, { method: 'GET', signal: options.signal })
  return handleResponse<T>(method, response)
}

/** POST {ApiURL}{method} s JSON tělem – náhrada MapService.GetResultFromApiPost. */
export async function apiPost<T>(method: string, body: unknown, options: RequestOptions = {}): Promise<T> {
  const response = await fetch(getConfig().apiUrl + method, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body ?? null),
    signal: options.signal,
  })
  return handleResponse<T>(method, response)
}

/** True, pokud chyba vznikla zrušením požadavku přes AbortController (není to skutečná chyba). */
export function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === 'AbortError'
}
