<script setup lang="ts">
// Components/Dialogs/Place/PlaceContentRouter.razor – seznam míst daného typu (+ filtr jmen u adres).
// Pozn.: v Blazoru měla ikona lupy u filtru překlep "onclik", takže nedělala nic; filtr se uplatní
// po potvrzení textu (událost change), stejně jako @bind-Value u RadzenTextBox.
import { computed, ref } from 'vue'
import { PlaceType } from '../../../models/enums'
import type { PlacesResult } from '../../../models/api'
import { t } from '../../../i18n'
import RzStack from '../../ui/RzStack.vue'
import IncidentPlace from './types/IncidentPlace.vue'
import InterestPlace from './types/InterestPlace.vue'
import AddressPlace from './types/AddressPlace.vue'
import MemoryPlace from './types/MemoryPlace.vue'
import MemorialPlace from './types/MemorialPlace.vue'

const props = defineProps<{ placesToShow: PlacesResult | null; placeType: string }>()

const filterInput = ref('')
const filteredName = ref('')

function byLabelCs<T extends { labelCs: string | null }>(items: T[] | null | undefined): T[] {
  return [...(items ?? [])].sort((a, b) => (a.labelCs ?? '').localeCompare(b.labelCs ?? '', 'cs'))
}

const addresses = computed(() =>
  props.placeType === PlaceType.Address ? props.placesToShow?.addresses : props.placesToShow?.addressesLastResidence,
)
</script>

<template>
  <template v-if="placesToShow">
    <template v-if="placeType === PlaceType.Incident">
      <IncidentPlace v-for="place in byLabelCs(placesToShow.incidents)" :key="place.id" :place="place" />
    </template>

    <template v-else-if="placeType === PlaceType.Interest">
      <InterestPlace v-for="(place, i) in byLabelCs(placesToShow.placesOfInterest)" :key="i" :place="place" />
    </template>

    <template v-else-if="placeType === PlaceType.Inaccessible">
      <InterestPlace v-for="(place, i) in byLabelCs(placesToShow.inaccessiblePlaces)" :key="i" :place="place" />
    </template>

    <template v-else-if="placeType === PlaceType.Address || placeType === PlaceType.AddressLastResidence">
      <RzStack v-if="addresses" orientation="horizontal" align-items="end">
        <input
          v-model="filterInput"
          class="rz-textbox"
          type="text"
          :placeholder="t('filterNames')"
          @change="filteredName = filterInput"
        />
        <img src="images/search-only-dark.svg" style="cursor: pointer" class="rz-pb-2" />
      </RzStack>
      <AddressPlace v-for="(house, i) in addresses ?? []" :key="i" :house="house" :filtered-name="filteredName" />
    </template>

    <template v-else-if="placeType === PlaceType.Memory">
      <MemoryPlace v-for="(place, i) in byLabelCs(placesToShow.placesOfMemory)" :key="i" :place="place" />
    </template>

    <template v-else-if="placeType === PlaceType.Memorial">
      <MemorialPlace v-for="place in byLabelCs(placesToShow.memorials)" :key="place.id" :place="place" />
    </template>
  </template>
</template>
