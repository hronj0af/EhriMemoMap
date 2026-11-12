<script setup lang="ts">
// Components/Dialogs/Victim/Victim.razor – detail oběti (údaje, místa, transporty, příbuzní, dokumenty).
import { computed, ref } from 'vue'
import { useMapStore } from '../../../stores/map'
import { MapType } from '../../../models/enums'
import { firstLetterToUpper, formatDate, nl2br } from '../../../models/helpers'
import { useI18n } from '../../../i18n'
import RzRow from '../../ui/RzRow.vue'
import RzColumn from '../../ui/RzColumn.vue'
import VictimPlace from './VictimPlace.vue'
import VictimShortInfo from '../shared/VictimShortInfo.vue'
import DocumentGallery from '../shared/DocumentGallery.vue'

const store = useMapStore()
const { t, byLang } = useI18n()

const relativesOpen = ref(false)
const showMoreOtherInfo = ref(false)

const victim = computed(() => store.victimLongInfo)

/** Nejbližší rodina: bratr (2), dcera (3), manžel (5), manželka (6), matka (7), otec (9), sestra (10), syn (13) */
const CLOSEST_FAMILY = [2, 3, 5, 6, 7, 9, 10, 13]
const closestFamily = computed(() =>
  (victim.value?.relatedPersons ?? []).filter((a) => a.relationshipToPersonType != null && CLOSEST_FAMILY.includes(a.relationshipToPersonType)),
)
const otherRelatives = computed(() =>
  (victim.value?.relatedPersons ?? []).filter((a) => a.relationshipToPersonType == null || !CLOSEST_FAMILY.includes(a.relationshipToPersonType)),
)

/**
 * Jméno rozdělené před posledním slovem – poslední slovo a ikona trajektorie se nezalomí od sebe
 * (jinak by ikona mohla zůstat sama na dalším řádku).
 */
const labelParts = computed(() => {
  const label = victim.value?.label ?? ''
  const i = label.lastIndexOf(' ')
  return i < 0 ? { head: '', tail: label } : { head: label.slice(0, i + 1), tail: label.slice(i + 1) }
})

const transports = computed(() => [...(victim.value?.transports ?? [])].sort((a, b) => a.id - b.id))
const otherInfo = computed(() => byLang(victim.value?.descriptionCs ?? null, victim.value?.descriptionEn ?? null))

function label(key: string) {
  return firstLetterToUpper(t(key))
}
</script>

<template>
  <div style="padding: 2.5rem 2.5rem 0 2.5rem">
    <template v-if="victim">
      <RzRow class="rz-mb-6" align-items="end" justify-content="space-between">
        <RzColumn :size="9">
          <h1>
            {{ labelParts.head }}<span class="victim-name-tail"
              >{{ labelParts.tail
            }}<button
              v-if="victim.trajectoryId != null && store.mapType === MapType.Trajectory"
              type="button"
              class="victim-trajectory-button"
              :title="t('showWholeTrajectory')"
              :aria-label="t('showWholeTrajectory')"
              @click="store.showWholeTrajectory()"
            >
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path
                  d="M12 2C8.1 2 5 5.1 5 9c0 5.3 7 13 7 13s7-7.7 7-13c0-3.9-3.1-7-7-7zm0 9.5a2.5 2.5 0 1 1 0-5 2.5 2.5 0 0 1 0 5z"
                />
              </svg>
            </button></span
            >
          </h1>
        </RzColumn>
        <RzColumn v-if="victim.narrativeMapId != null" :size="3" style="text-align: right">
          <img
            style="width: 30px; cursor: pointer"
            :title="t('viewStoryMap') + '...'"
            src="images/storymap-icon.png"
            @click="store.showNarrativeMap(victim.narrativeMapId)"
          />
        </RzColumn>
      </RzRow>

      <table class="victim">
        <tbody>
          <template v-if="victim.places != null">
            <tr v-if="victim.maidenname">
              <td>{{ label('maidenname') }}</td>
              <td>{{ victim.maidenname }}</td>
            </tr>
            <tr v-if="victim.title">
              <td>{{ label('personTitle') }}</td>
              <td>{{ victim.title }}</td>
            </tr>
            <tr v-if="victim.birthDate != null || victim.birthDateText">
              <td>{{ label('birthDate') }}</td>
              <td>{{ victim.birthDateText ?? formatDate(victim.birthDate) }}</td>
            </tr>
            <!-- místo narození, bydliště k 13. 5. 1939, poslední bydliště před deportací, poslední bydliště, další bydliště -->
            <VictimPlace :type="22" :show-dates="false" />
            <VictimPlace :type="24" :show-dates="false" />
            <VictimPlace :type="26" :show-dates="false" />
            <VictimPlace :type="27" />
            <VictimPlace :type="36" resource-name-type="otherResidence" />
            <tr v-if="transports.length > 0">
              <td>{{ label('deportation') }}</td>
              <td>
                <ul style="margin: 0; list-style-type: none; padding: 0">
                  <li v-for="transport in transports" :key="transport.id">
                    {{ transport.code }}
                    ({{ formatDate(transport.date) }},
                    {{ byLang(transport.fromCs, transport.fromEn) }}
                    →
                    {{ byLang(transport.toCs, transport.toEn) }})
                  </li>
                </ul>
              </td>
            </tr>
            <!-- místo věznění -->
            <VictimPlace :type="28" />
            <tr v-if="victim.fateCs">
              <td>{{ label('fate') }}</td>
              <td>{{ byLang(victim.fateCs, victim.fateEn) }}</td>
            </tr>
            <!-- místo osvobození -->
            <VictimPlace :type="29" />
            <tr v-if="victim.deathDate != null || victim.deathDateText">
              <td>{{ label('deathDate') }}</td>
              <td>{{ victim.deathDateText ?? formatDate(victim.deathDate) }}</td>
            </tr>
            <!-- místo úmrtí -->
            <VictimPlace :type="23" />
            <tr v-if="otherInfo">
              <td>{{ label('otherInfo') }}</td>
              <td>
                <template v-if="otherInfo.length > 200">
                  <template v-if="!showMoreOtherInfo">
                    <span v-html="nl2br(otherInfo.substring(0, 200)) + '...'"></span>
                    <span>
                      <a style="cursor: pointer" @click="showMoreOtherInfo = true">{{ t('more') }}</a>
                    </span>
                  </template>
                  <template v-else>
                    <span v-html="nl2br(otherInfo)"></span>
                    <span>
                      <a style="cursor: pointer" @click="showMoreOtherInfo = false">{{ t('less') }}</a>
                    </span>
                  </template>
                </template>
                <span v-else v-html="nl2br(otherInfo)"></span>
              </td>
            </tr>
          </template>
        </tbody>
      </table>

      <template v-if="victim.relatedPersons && victim.relatedPersons.length > 0">
        <template v-if="closestFamily.length > 0">
          <h3 class="rz-mt-8">{{ t('relatedClosestFamily') }}</h3>
          <RzRow class="rz-mt-4" gap="1rem" row-gap="2rem" justify-content="start">
            <VictimShortInfo v-for="(person, i) in closestFamily" :key="i" :victim="person" small relative />
          </RzRow>
        </template>
        <template v-if="otherRelatives.length > 0">
          <div class="rz-mt-4" style="cursor: pointer" @click="relativesOpen = !relativesOpen">
            <h4>
              {{ t('relatedOtherRelatives') }}
              &nbsp;&nbsp;
              <img
                v-if="!relativesOpen"
                style="height: 17px; cursor: pointer"
                src="images/arrow-right.png"
                :title="t('showOtherRelatives') + '...'"
              />
              <img v-else style="width: 17px; cursor: pointer" src="images/arrow-down.png" :title="t('hideOtherRelatives') + '...'" />
            </h4>
          </div>
          <RzRow v-if="relativesOpen" class="rz-mt-4" gap="1rem" row-gap="2rem" justify-content="start">
            <VictimShortInfo v-for="(person, i) in otherRelatives" :key="i" :victim="person" small relative />
          </RzRow>
        </template>
      </template>

      <DocumentGallery :documents="victim.documents" />
    </template>
  </div>
</template>
