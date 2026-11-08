<script setup lang="ts">
// Mapová stránka – přepis EhriMemoMap.Client/Pages/Map.razor.
// Postranní panel a modální dialogy se přes Teleport vykreslují do <body> (jako RadzenDialog v MainLayout).
import { computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useMapStore } from '../stores/map'
import { DialogType, MapState, ModalType } from '../models/enums'
import { mapApi, setMapEventHandlers } from '../map/mapApi'
import { setMapController } from '../map/controller'
import { getSelectedPlaceFromUrl, getUrlParam } from '../utils/url'
import { isMobileView } from '../utils/dom'
import { t } from '../i18n'
import ControlButtonsWrapper from '../components/buttonWrappers/ControlButtonsWrapper.vue'
import ZoomButtonsWrapper from '../components/buttonWrappers/ZoomButtonsWrapper.vue'
import ContextButtonsWrapper from '../components/buttonWrappers/ContextButtonsWrapper.vue'
import MobileTopBarWrapper from '../components/buttonWrappers/MobileTopBarWrapper.vue'
import TimelineMapWrapper from '../components/dialogs/timeline/TimelineMapWrapper.vue'
import SideDialog from '../components/ui/SideDialog.vue'
import ModalDialog from '../components/ui/ModalDialog.vue'

const props = defineProps<{ city?: string }>()
const store = useMapStore()
const router = useRouter()

const cityName = computed(() => props.city || 'prague')

onMounted(async () => {
  document.title = 'MemoMap ' + t('city_' + cityName.value)

  // ovládání mapy pro store + volání z mapy do storu (náhrada DotNetObjectReference)
  setMapController(mapApi)
  setMapEventHandlers({
    refreshObjectsOnMap: store.onMapMoved,
    showPlaceInfo: store.showPlaceInfo,
    setMobileView: store.setMobileView,
    windowResized: store.onWindowResize,
    openStoryMapTimelineLabel: store.openStoryMapTimelineLabel,
  })

  // rozměry okna kvůli dialogu, mobilní zobrazení
  store.windowWidth = window.innerWidth
  store.windowHeight = window.innerHeight
  store.setMobileView(isMobileView())
  // dokument se na mapové stránce neposouvá (styles/map-page.css)
  document.documentElement.classList.add('map-page')

  // vrstvy a bod časové osy z URL
  const layers = getUrlParam('layers')
  const timelinePointName = getUrlParam('timeline')

  if (!(await store.init(cityName.value, layers, timelinePointName))) {
    // neznámé město (neexistuje mapsettings.{city}.json) → úvodní stránka (Blazor tu spadl s chybou)
    console.warn(`Mapa "${cityName.value}" neexistuje – přesměrování na úvodní stránku.`)
    await router.replace('/')
    return
  }
  try {
    await store.getAllNarrativeMaps()
  } catch (error) {
    console.error('Načtení příběhových map selhalo', error)
  }

  const bounds = getUrlParam('bounds')

  if (store.appState === 'Shutdown') {
    store.modal = ModalType.Error
    return
  }

  const coordinates = getSelectedPlaceFromUrl()
  const zoomStr = getUrlParam('zoom')
  const parsedZoom = zoomStr ? parseFloat(zoomStr) : NaN
  const initialZoom = Number.isFinite(parsedZoom) ? parsedZoom : 0

  // uvítací dialog jen pokud uživatel s mapou ještě nepracoval (v URL nejsou bounds ani vybrané místo)
  if (!bounds && coordinates == null) {
    if (!store.isMobileView) store.modal = ModalType.Welcome
    else await store.setDialog(DialogType.Welcome)
  }

  // inicializace Leaflet mapy
  mapApi.initMap(store.getMapInfoForLeaflet(Math.round(initialZoom)))

  store.mapZoom = initialZoom === 0 ? mapApi.getZoom() : initialZoom
  store.setBBox(mapApi.getMapBounds())

  if (coordinates != null) await store.showPlaceInfo(store.mapZoom, coordinates, 0)

  await store.refreshObjectsOnMap(true)
  mapApi.showLayersForNormalMap()
  // opětovné přidání vrstev do mapy vrátí markerům výchozí třídy – vybrané místo z odkazu znovu zvýraznit
  if (store.selectedPointGuids?.length) mapApi.selectPointOnMap(store.selectedPointGuids)
  store.mapState = MapState.Rendered

  // rozměry mapy podle otevřeného panelu (Blazor tu volal fitMapToWindow() bez parametrů, takže při
  // otevření odkazu s vybraným místem byla mapa na mobilu i pod panelem a místo mohlo být schované)
  store.fitMapToDialog()
})

onUnmounted(() => {
  document.documentElement.classList.remove('map-page')
  store.destroy()
  setMapEventHandlers(null)
  setMapController(null)
})
</script>

<template>
  <template v-if="store.appState !== 'Shutdown'">
    <div id="map" style="cursor: default">
      <!-- filtr pro hezčí vykreslení hranic čtvrtí / celé Prahy (site.css: div.leaflet-overlay-pane svg > g path.statistics-polygon) -->
      <svg xmlns="http://www.w3.org/2000/svg" version="1.1">
        <defs>
          <filter id="inset-shadow">
            <feOffset dx="0" dy="0" />
            <feGaussianBlur stdDeviation="5" result="offset-blur" />
            <feComposite operator="out" in="SourceGraphic" in2="offset-blur" result="inverse" />
          </filter>
        </defs>
      </svg>
    </div>

    <ControlButtonsWrapper />
    <ZoomButtonsWrapper />
    <ContextButtonsWrapper />
    <TimelineMapWrapper />
    <MobileTopBarWrapper />
  </template>
  <Teleport to="body">
    <SideDialog />
    <ModalDialog />
  </Teleport>
</template>
