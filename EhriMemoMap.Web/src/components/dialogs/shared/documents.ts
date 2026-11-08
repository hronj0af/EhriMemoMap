// Pomocné funkce pro galerii dokumentů (DocumentGallery.razor, VictimShortInfo.razor).
import type { OmekaUrl } from '../../../models/api'

let counter = 0
/** Jedinečný identifikátor pro id elementů galerie (náhrada Guid.NewGuid()). */
export function uniqueId(prefix = 'g'): string {
  counter++
  return `${prefix}${Date.now().toString(36)}${counter}`
}

function mimeTypeFromUrl(url: string | null): string {
  const ext = (url ?? '').split('?')[0].split('.').pop()?.toLowerCase()
  switch (ext) {
    case 'webm':
      return 'video/webm'
    case 'mp4':
      return 'video/mp4'
    case 'mp3':
      return 'audio/mp3'
    case 'ogv':
    case 'ogg':
      return 'video/ogg'
    default:
      return 'video/mp4'
  }
}

/** JSON se zdroji videa pro BiggerPicture (atribut data-sources) – GetSourcesJson. */
export function sourcesJson(url: OmekaUrl | null | undefined): string {
  if (!url) return '[]'
  return JSON.stringify([{ src: url.url, type: mimeTypeFromUrl(url.url) }])
}

export function isImageDocument(type: string | null): boolean {
  return type === 'image' || type === 'text'
}
