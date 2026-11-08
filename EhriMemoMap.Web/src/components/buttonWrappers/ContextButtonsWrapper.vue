<script setup lang="ts">
// Components/ButtonWrappers/ContextButtonsWrapper.razor – rychlé přepínače vrstev (jen desktop).
import { useMapStore } from '../../stores/map'
import { MapState } from '../../models/enums'
import LayersQuickAccessButtons from '../buttons/LayersQuickAccessButtons.vue'

const store = useMapStore()
</script>

<template>
  <LayersQuickAccessButtons v-if="store.mapState === MapState.Rendered && !store.isMobileView" />
</template>
