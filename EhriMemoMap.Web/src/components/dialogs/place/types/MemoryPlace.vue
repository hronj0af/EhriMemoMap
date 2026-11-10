<script setup lang="ts">
// Components/Dialogs/Place/Types/Memory.razor – místa paměti (Stolpersteine, pamětní desky…).
import type { PlaceMemory } from '../../../../models/api'
import { formatDate, nl2br } from '../../../../models/helpers'
import { useI18n } from '../../../../i18n'
import RzRow from '../../../ui/RzRow.vue'
import RzCard from '../../../ui/RzCard.vue'
import DocumentGallery from '../../shared/DocumentGallery.vue'

defineProps<{ place: PlaceMemory }>()
const { t, byLang } = useI18n()
</script>

<template>
  <RzRow>
    <RzCard style="width: 100%">
      <template v-if="place.city !== 'prague'">
        <h3 v-if="place.type === 'stolpersteine_set'">{{ t('memoryType_' + place.type) }}</h3>
        <h3 v-else>{{ byLang(place.labelCs, place.labelEn) ?? '' }}</h3>
      </template>
      <h2 v-else>{{ t('memoryType_' + place.type) }}</h2>

      <h5 class="rz-mb-4">{{ byLang(place.addressCs, place.addressEn) }}</h5>
      <p v-if="place.descriptionCs" class="rz-mb-4" v-html="nl2br(byLang(place.descriptionCs, place.descriptionEn))"></p>

      <template v-if="place.city !== 'prague'">
        <p v-if="place.creationDate" :class="place.inscriptionCs ? 'rz-mb-4' : ''">
          <b>{{ t('date_of_laying') }}:</b> {{ formatDate(place.creationDate) }}
        </p>
        <p v-if="place.inscriptionCs">
          <b>{{ t('inscription') }}</b>:<br /><span v-html="nl2br(byLang(place.inscriptionCs, place.inscriptionEn))"></span>
        </p>
      </template>
    </RzCard>
  </RzRow>

  <template v-for="(item, i) in place.items ?? []" :key="item.id">
    <RzRow class="rz-mb-2 rz-mt-2">
      <RzCard style="width: 100%">
        <h3>{{ item.labelCs }}</h3>
        <ul v-if="place.city === 'prague'" class="no-bullets">
          <li>
            <a :href="item.linkStolpersteineCs ?? undefined" target="_blank">www.stolpersteinecz.cz</a>
          </li>
          <li>
            <a :href="item.linkHolocaustCs ?? undefined" target="_blank">www.holocaust.cz</a>
          </li>
        </ul>
        <template v-if="place.city !== 'prague'">
          <p v-if="item.creationDate" :class="item.inscriptionCs ? 'rz-mb-4' : ''">
            <b>{{ t('date_of_laying') }}:</b> {{ formatDate(item.creationDate) }}
          </p>
          <p v-if="item.inscriptionCs">
            <b>{{ t('inscription') }}</b>:<br /><span v-html="nl2br(byLang(item.inscriptionCs, item.inscriptionEn))"></span>
          </p>
        </template>
      </RzCard>
    </RzRow>
    <div v-if="i < (place.items?.length ?? 0) - 1" class="rz-my-2">&nbsp;</div>
  </template>

  <div class="rz-mb-4">
    <DocumentGallery :documents="place.documents" />
  </div>
</template>
