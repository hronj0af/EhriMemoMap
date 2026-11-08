<script setup lang="ts">
// Components/Dialogs/Place/Types/Interest.razor – body zájmu a nepřístupná místa.
import type { PlaceInterest } from '../../../../models/api'
import { useMapStore } from '../../../../stores/map'
import { nl2br } from '../../../../models/helpers'
import { useI18n } from '../../../../i18n'
import RzRow from '../../../ui/RzRow.vue'
import RzCard from '../../../ui/RzCard.vue'
import DocumentGallery from '../../shared/DocumentGallery.vue'

defineProps<{ place: PlaceInterest }>()
const store = useMapStore()
const { t, byLang } = useI18n()
</script>

<template>
  <RzRow class="rz-mb-4">
    <RzCard style="width: 100%">
      <div style="display: flex; justify-content: space-between; align-items: flex-end">
        <div style="flex: 1">
          <h3 style="margin-bottom: 4px">{{ byLang(place.labelCs, place.labelEn) }}</h3>
        </div>
        <div v-if="place.narrativeMapId != null" style="flex-shrink: 0; margin-left: 12px">
          <img
            src="images/storymap-icon.png"
            alt="Story Map"
            :title="t('storyMapExistsForPlace')"
            style="width: 30px; cursor: pointer"
            @click="store.showNarrativeMap(place.narrativeMapId)"
          />
        </div>
      </div>
      <h5 class="rz-mb-4" style="margin-top: 0">{{ byLang(place.addressCs, place.addressEn) }}</h5>
      <p class="rz-mb-4" v-html="nl2br(byLang(place.descriptionCs, place.descriptionEn))"></p>
      <DocumentGallery :documents="place.documents" />
    </RzCard>
  </RzRow>
</template>
