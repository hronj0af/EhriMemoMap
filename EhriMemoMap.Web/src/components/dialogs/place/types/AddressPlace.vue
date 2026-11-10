<script setup lang="ts">
// Components/Dialogs/Place/Types/Address.razor – adresa s obyvateli (seznam podle města).
import { computed } from 'vue'
import type { AddressWithVictims } from '../../../../models/api'
import { useMapStore } from '../../../../stores/map'
import { useI18n } from '../../../../i18n'
import { anyMatchesFilter } from './victimFilter'
import PragueHouse from './addressTypes/PragueHouse.vue'
import PragueVictimList from './addressTypes/PragueVictimList.vue'
import PacovVictimList from './addressTypes/PacovVictimList.vue'
import RicanyVictimList from './addressTypes/RicanyVictimList.vue'

const props = defineProps<{ house: AddressWithVictims | null; filteredName?: string }>()
const store = useMapStore()
const { t, byLang } = useI18n()

const city = computed(() => store.city ?? '')
const showAddress = computed(() => !!props.house?.address && anyMatchesFilter(props.house.victims, props.filteredName))
</script>

<template>
  <template v-if="house">
    <template v-if="showAddress && house.address">
      <h3 class="rz-mt-8 rz-mb-4">{{ byLang(house.address.cs, house.address.en) }}</h3>
      <hr />
      <h4 v-if="house.address.de" class="rz-mb-2 rz-mt-4">
        {{ house.address.de }} <span class="rz-text-caption">({{ t('addressOccupation') }})</span>
      </h4>
      <h4 v-if="house.address.currentCs" class="rz-mb-8">
        {{ byLang(house.address.currentCs, house.address.currentEn) }}
        <span class="rz-text-caption">({{ t('addressCurrent') }})</span>
      </h4>
    </template>

    <template v-if="city.includes('prague')">
      <PragueHouse :house="house" />
      <PragueVictimList :victims="house.victims" :filtered-name="filteredName" />
    </template>
    <PacovVictimList v-else-if="city.includes('pacov')" :victims="house.victims" :filtered-name="filteredName" />
    <!-- Říčany a ostatní města z databáze MemoMap (např. Velké Meziříčí): obecný abecední seznam obětí -->
    <RicanyVictimList v-else :victims="house.victims" :filtered-name="filteredName" />
  </template>
</template>
