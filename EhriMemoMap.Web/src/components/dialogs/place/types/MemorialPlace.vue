<script setup lang="ts">
// Components/Dialogs/Place/Types/Memorial.razor – pomníky.
import { computed } from 'vue'
import type { PlaceMemorial } from '../../../../models/api'
import { formatDate, nl2br } from '../../../../models/helpers'
import { useI18n } from '../../../../i18n'
import RzRow from '../../../ui/RzRow.vue'
import RzCard from '../../../ui/RzCard.vue'
import DocumentGallery from '../../shared/DocumentGallery.vue'

const props = defineProps<{ place: PlaceMemorial }>()
const { byLang } = useI18n()
const link = computed(() => byLang(props.place.linkCs, props.place.linkEn))
</script>

<template>
  <RzRow class="rz-mb-4">
    <RzCard style="width: 100%">
      <h3>{{ byLang(place.labelCs, place.labelEn) }}</h3>
      <h5 class="rz-mb-4">{{ formatDate(place.date) }}, {{ byLang(place.addressCs, place.addressEn) }}</h5>
      <p v-html="nl2br(byLang(place.descriptionCs, place.descriptionEn))"></p>
      <p v-if="link">
        <a :href="link" target="_blank">{{ link }}</a>
      </p>
      <DocumentGallery :documents="place.documents" />
    </RzCard>
  </RzRow>
</template>
