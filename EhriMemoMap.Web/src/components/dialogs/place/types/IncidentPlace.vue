<script setup lang="ts">
// Components/Dialogs/Place/Types/Incident.razor – porušení protižidovských nařízení.
import { computed } from 'vue'
import type { PlaceIncident } from '../../../../models/api'
import { useMapStore } from '../../../../stores/map'
import { useI18n } from '../../../../i18n'
import RzRow from '../../../ui/RzRow.vue'
import RzColumn from '../../../ui/RzColumn.vue'
import RzStack from '../../../ui/RzStack.vue'
import RzCard from '../../../ui/RzCard.vue'

const props = defineProps<{ place: PlaceIncident }>()
const store = useMapStore()
const { t, byLang, culture } = useI18n()

const date = computed(() => byLang(props.place.dateCs, props.place.dateEn))
const address = computed(() => (date.value ? ', ' : '') + (byLang(props.place.addressCs, props.place.addressEn) ?? ''))
</script>

<template>
  <RzRow class="rz-mb-4">
    <RzCard style="width: 100%">
      <h3>{{ byLang(place.labelCs, place.labelEn) }}</h3>
      <h5 class="rz-mb-4">
        <span v-if="date">{{ date }}</span>
        <span v-if="address">{{ address }}</span>
      </h5>
      <ul class="no-bullets rz-mt-4">
        <li v-if="place.descriptionCs">{{ t('description') }}: {{ byLang(place.descriptionCs, place.descriptionEn) }}</li>
        <li v-if="place.typeCs">{{ t('type') }}: {{ byLang(place.typeCs, place.typeEn) }}</li>
        <li v-if="place.specificationCs">{{ t('specification') }}: {{ byLang(place.specificationCs, place.specificationEn) }}</li>
      </ul>
      <RzRow v-if="place.documents && place.documents.length > 0" class="rz-mt-2 rz-mb-4">
        <RzColumn v-for="doc in place.documents" :key="doc.id" :size="store.isMobileView ? 3 : 2">
          <a
            :href="(culture === 'cs-CZ' ? doc.documentUrlCs : doc.documentUrlEn) ?? undefined"
            target="_blank"
            style="text-decoration: none; color: black"
          >
            <RzCard class="document-card">
              <RzStack orientation="vertical" align-items="start" gap="5px">
                <div style="position: relative; width: 100%">
                  <img class="document-photo" :src="doc.url?.[0]?.url ?? undefined" />
                  <img class="document-forward-icon" src="/images/forward.svg" />
                </div>
              </RzStack>
            </RzCard>
          </a>
        </RzColumn>
      </RzRow>
    </RzCard>
  </RzRow>
</template>
