// DOM pomocné funkce z helpers.js a site.ts (galerie BiggerPicture, výška fotek, detekce mobilu).

interface BiggerPictureInstance {
  open(options: { items: NodeListOf<Element> | Element[]; el?: Element; intro?: string }): void
  close(): void
}
type BiggerPictureFactory = (options: { target: HTMLElement }) => BiggerPictureInstance
declare global {
  interface Window {
    BiggerPicture?: BiggerPictureFactory
  }
}

let bp: BiggerPictureInstance | null = null

let bpLoading: Promise<BiggerPictureFactory | undefined> | null = null

/**
 * Načte BiggerPicture až při prvním otevření galerie (src/vendor, převedené legacy buildem).
 * Bundler bere UMD obal jako CommonJS a knihovnu vrátí jako default export; globální proměnná je jen záloha.
 */
function loadBiggerPicture(): Promise<BiggerPictureFactory | undefined> {
  bpLoading ??= import('../vendor/bigger-picture.js').then(
    (mod) => (mod.default as BiggerPictureFactory | undefined) ?? window.BiggerPicture,
  )
  return bpLoading
}

/** Otevře galerii BiggerPicture z odkazů v kontejneru #documents_{id} (helpers.js initGallery). */
export async function initGallery(id: string | number, index = 0): Promise<void> {
  try {
    const container = document.getElementById('documents_' + id)
    if (!container) return
    const BiggerPicture = await loadBiggerPicture()
    if (typeof BiggerPicture !== 'function') {
      console.error('BiggerPicture se nepodařilo načíst.')
      return
    }
    bp?.close()
    bp = BiggerPicture({ target: document.body })
    const links = container.querySelectorAll('a')
    if (!links || links.length === 0) return
    bp.open({ items: links, el: links[index], intro: 'fadeup' })
  } catch (err) {
    console.error('initGallery error', err)
  }
}

/** helpers.js closeGallery */
export function closeGallery(): void {
  bp?.close()
}

/** helpers.js resetPageElementHeight – úvodní stránka zruší výšku nastavenou mapou */
export function resetPageElementHeight(): void {
  const page = document.getElementsByClassName('page')[0] as HTMLElement | undefined
  if (page) page.style.height = ''
}

/** helpers.js adjustDocumentContainerWidths – šířka kontejneru dokumentu podle načtené miniatury */
export function adjustDocumentContainerWidths(guid: string): void {
  setTimeout(() => {
    document.querySelectorAll<HTMLElement>(`[id^="doc-container-${guid}-"]`).forEach((container) => {
      const img = container.querySelector<HTMLImageElement>('.document-photo')
      if (!img) return
      if (img.complete) container.style.width = img.offsetWidth + 'px'
      else img.onload = () => (container.style.width = img.offsetWidth + 'px')
    })
  }, 100)
}

/** site.ts resizePhotoHeight – fotky obětí v poměru 0,875 */
export function resizePhotoHeight(): void {
  const photos = document.querySelectorAll<HTMLElement>('.victim-photo')
  if (photos.length === 0) return
  const photoHeight = photos[0].clientWidth / 0.875
  photos.forEach((item) => (item.style.height = photoHeight + 'px'))
}

let isMobileDevice: boolean | null = null

/** site.ts isMobileView – úzké okno, nebo mobilní zařízení podle user agenta */
export function isMobileView(): boolean {
  if (window.innerWidth < 768) return true
  if (isMobileDevice === null) {
    isMobileDevice = /Android|webOS|iPhone|iPad|iPod|BlackBerry|Windows Phone/i.test(navigator.userAgent)
  }
  return isMobileDevice
}
