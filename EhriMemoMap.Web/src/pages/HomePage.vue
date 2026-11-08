<script setup lang="ts">
// Úvodní stránka – přepis EhriMemoMap.Client/Pages/Home.razor.
// Karty měst (appsettings App.Cities) + metodiky; ?detail=main|metodiky|{město} zobrazí detail.
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getConfig } from '../config'
import { t } from '../i18n'
import { useMapStore } from '../stores/map'
import { resetPageElementHeight } from '../utils/dom'
import RzRow from '../components/ui/RzRow.vue'
import RzColumn from '../components/ui/RzColumn.vue'
import RzCard from '../components/ui/RzCard.vue'
import RzButton from '../components/ui/RzButton.vue'
import RzIcon from '../components/ui/RzIcon.vue'
import LanguageButton from '../components/buttons/LanguageButton.vue'
import JoinButton from '../components/buttons/JoinButton.vue'

const route = useRoute()
const router = useRouter()
const store = useMapStore()
const cities = getConfig().cities

interface Download {
  file: string
  label: string
}
interface Methodology {
  title: string
  target: string
  description: string
  downloads: Download[]
}

/** Metodiky ke stažení (soubory v public/metodiky) */
const methodologies: Methodology[] = [
  {
    title: 'metodiky1_title',
    target: 'metodiky1_target',
    description: 'metodiky1_desc',
    downloads: [
      { file: 'metodiky/Mesto_jako_nastroj_moci/ATX_EHRI_METODIKA_MESTO%20JAKO%20NASTROJ_metodick%C3%BD_materi%C3%A1l.pdf', label: 'metodiky_dl_material' },
      { file: 'metodiky/Mesto_jako_nastroj_moci/ATX_EHRI_METODIKA_MESTO%20JAKO%20NASTROJ_pracovni%20list.pdf', label: 'metodiky_dl_worksheet' },
      { file: 'metodiky/Mesto_jako_nastroj_moci/ATX_EHRI_METODIKA_MESTO%20JAKO%20NASTROJ_doprovodne%20materialy.pdf', label: 'metodiky_dl_archival' },
    ],
  },
  {
    title: 'metodiky2_title',
    target: 'metodiky2_target',
    description: 'metodiky2_desc',
    downloads: [
      { file: 'metodiky/Nebyt_soucasti_mista/ATX_EHRI_METODIKA_NEBYT%20SOUCASTI%20MISTA_metodick%C3%BD_materi%C3%A1l.pdf', label: 'metodiky_dl_material' },
      { file: 'metodiky/Nebyt_soucasti_mista/ATX_EHRI_METODIKA_NEBYT%20SOUCASTI%20MISTA_doprovodny%20text.pdf', label: 'metodiky_dl_background' },
      { file: 'metodiky/Nebyt_soucasti_mista/ATX_EHRI_METODIKA_NEBYT%20SOUCASTI%20MISTA_pracovni%20list.pdf', label: 'metodiky_dl_worksheet' },
      { file: 'metodiky/Nebyt_soucasti_mista/ATX_EHRI_METODIKA_NEBYT%20SOUCASTI%20MISTA_doprovodne%20materialy.pdf', label: 'metodiky_dl_archival' },
    ],
  },
  {
    title: 'metodiky3_title',
    target: 'metodiky3_target',
    description: 'metodiky3_desc',
    downloads: [
      { file: 'metodiky/Pravo_v_dobe_bezpravi/ATX_EHRI_METODIKA_PRAVO%20V%20DOBE%20BEZPRAVI_metodick%C3%BD_materi%C3%A1l.pdf', label: 'metodiky_dl_material' },
      { file: 'metodiky/Pravo_v_dobe_bezpravi/ATX_EHRI_METODIKA_PRAVO%20V%20DOBE%20BEZPRAVI_doprovodny%20text_1.pdf', label: 'metodiky_dl_background1' },
      { file: 'metodiky/Pravo_v_dobe_bezpravi/ATX_EHRI_METODIKA_PRAVO%20V%20DOBE%20BEZPRAVI_doprovodny%20text_2.pdf', label: 'metodiky_dl_background2' },
      { file: 'metodiky/Pravo_v_dobe_bezpravi/ATX_EHRI_METODIKA_PRAVO%20V%20DOBE%20BEZPRAVI_pracovni%20list.pdf', label: 'metodiky_dl_worksheet' },
      { file: 'metodiky/Pravo_v_dobe_bezpravi/ATX_EHRI_METODIKA_PRAVO%20V%20DOBE%20BEZPRAVI_doprovodne%20materialy.pdf', label: 'metodiky_dl_archival' },
    ],
  },
]

/** Text s fallbackem jako v C#: pokud klíč neexistuje (t vrací klíč), zkusí se klíč z původní hodnoty parametru. */
function tWithFallback(key: string, fallbackKey: string): string {
  const value = t(key)
  return value === key ? t(fallbackKey) : value
}

/** UpdateDetailFromParameter */
const detail = computed(() => {
  const raw = typeof route.query.detail === 'string' ? route.query.detail : null
  const clean = raw?.trim().toLowerCase() || null
  if (!clean) return null
  if (clean === 'main') return { kind: 'text' as const, title: '', content: t('welcomeText_long') }
  if (clean === 'metodiky') return { kind: 'metodiky' as const, title: t('metodiky_subtitle'), content: '' }
  return {
    kind: 'text' as const,
    title: tWithFallback('city_' + clean, 'city_' + raw),
    content: tWithFallback('welcomeText_' + clean + '_long', 'welcomeText_' + raw + '_long'),
  }
})

const showDetail = (value: string | null) => router.push(value ? { path: '/', query: { detail: value } } : '/')

function cityCardStyle(city: string) {
  return `background-image: linear-gradient(rgba(255, 255, 255, 0.7) 100%), url('images/welcome_image_${city.toLowerCase()}.jpg'); background-size: cover; background-position: center;`
}

onMounted(() => {
  document.title = 'MemoMap'
  store.destroy()
  resetPageElementHeight()
})
</script>

<template>
  <div style="position: fixed; top: 95px; right: 0px; width: 30px; z-index: 1000">
    <LanguageButton />
  </div>
  <div style="position: fixed; top: 35px; right: 17px; width: 30px; z-index: 1000">
    <JoinButton />
  </div>

  <template v-if="detail">
    <div style="position: fixed; top: 20px; left: 0px; width: 100px; z-index: 1000">
      <RzButton
        icon="arrow_back"
        class="back-btn"
        variant="text"
        button-style="secondary"
        style="border-radius: 0 4px 4px 0"
        @click="showDetail(null)"
      />
    </div>
    <div class="detail-container">
      <h1 class="main-title">{{ t('memoMaps') }}</h1>

      <template v-if="detail.kind === 'metodiky'">
        <h2 class="detail-subtitle">{{ t('metodiky_subtitle') }}</h2>
        <div class="detail-content metodiky-content">
          <p>{{ t('metodiky_intro1') }}</p>
          <p v-html="t('metodiky_intro2')"></p>
          <p v-html="t('metodiky_intro3')"></p>
          <p class="metodiky-intro-note">
            <strong>{{ t('metodiky_intro_note') }}</strong>
          </p>
          <div class="metodiky-cards">
            <div v-for="m in methodologies" :key="m.title" class="metodika-card">
              <h3 class="metodika-title" v-html="t(m.title)"></h3>
              <div class="metodika-target">
                <strong>{{ t('metodiky_target_label') }}</strong> {{ t(m.target) }}
              </div>
              <p class="metodika-desc">{{ t(m.description) }}</p>
              <h4 class="download-title">{{ t('metodiky_download_title') }}</h4>
              <div class="download-list">
                <a v-for="d in m.downloads" :key="d.file" :href="d.file" download class="download-button" target="_blank">
                  <RzIcon icon="picture_as_pdf" /> {{ t(d.label) }}
                </a>
              </div>
            </div>
          </div>
          <div class="metodiky-logos">
            <a href="https://www.antikomplex.cz/" target="_blank" rel="noopener noreferrer" title="Antikomplex">
              <img src="images/antikomplex-logo.png" alt="Antikomplex logo" class="partner-logo" />
            </a>
            <a href="https://www.ehri.cz/" target="_blank" rel="noopener noreferrer" title="EHRI-CZ">
              <img src="images/ehri-cz-logo2.png" alt="EHRI-CZ logo" class="partner-logo" />
            </a>
          </div>
        </div>
      </template>
      <template v-else>
        <h2 v-if="detail.title" class="detail-subtitle">{{ detail.title }}</h2>
        <div class="detail-content" v-html="detail.content"></div>
      </template>
    </div>
  </template>

  <div v-else class="home-container">
    <h1 class="main-title">{{ t('memoMaps') }}</h1>
    <div class="intro-text">
      <div v-html="t('welcomeText_short')"></div>
      <div class="read-more-container" style="display: flex; gap: 1rem; justify-content: center; flex-wrap: wrap">
        <RzButton :text="t('moreInfo')" class="info-btn" variant="text" button-style="secondary" icon="info" @click="showDetail('main')" />
      </div>
    </div>
    <RzRow gap="1rem">
      <RzColumn v-for="city in cities" :key="city" :size-lg="6" :size-sm="12" :size-md="6">
        <RzCard style="height: 200px" class="rz-mb-12 rz-mt-4">
          <div class="city-card" :style="cityCardStyle(city)">
            <h2 class="city-title" style="cursor: pointer" @click="router.push('/' + city)">
              {{ t('city_' + city) }}
              <RzButton icon="arrow_forward" variant="text" button-style="secondary" style="vertical-align: middle; margin-left: 0.5rem" />
            </h2>
            <p class="city-description" v-html="t('welcomeText_' + city + '_short')"></p>
            <div class="city-buttons">
              <RzButton :text="t('moreInfo')" class="info-btn" variant="text" button-style="secondary" icon="info" @click="showDetail(city)" />
            </div>
          </div>
        </RzCard>
      </RzColumn>
      <RzColumn :size-lg="6" :size-sm="12" :size-md="6">
        <RzCard style="height: 200px" class="rz-mb-12 rz-mt-4">
          <div
            class="city-card"
            style="
              background-image: linear-gradient(rgba(255, 230, 227, 0.75) 100%), url('images/welcome_image_metodiky.jpg');
              background-size: cover;
              background-position: center;
            "
          >
            <h2 class="city-title" style="cursor: pointer" @click="showDetail('metodiky')">
              {{ t('metodiky') }}
              <RzButton icon="arrow_forward" variant="text" button-style="secondary" style="vertical-align: middle; margin-left: 0.5rem" />
            </h2>
            <p class="city-description">{{ t('metodiky_card_desc') }}</p>
          </div>
        </RzCard>
      </RzColumn>
    </RzRow>
  </div>
</template>
