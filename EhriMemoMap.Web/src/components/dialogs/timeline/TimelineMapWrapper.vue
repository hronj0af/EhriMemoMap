<script setup lang="ts">
// Components/Dialogs/Timeline/TimelineMapWrapper.razor – časová osa dole v mapě (desktop, běžná mapa).
import { useMapStore } from '../../../stores/map'
import { MapType } from '../../../models/enums'
import TimelineView from './TimelineView.vue'

const store = useMapStore()
</script>

<template>
  <div v-if="!store.isMobileView && store.mapType === MapType.Normal" class="timeline-wrapper">
    <TimelineView :width="550" />
  </div>
</template>
