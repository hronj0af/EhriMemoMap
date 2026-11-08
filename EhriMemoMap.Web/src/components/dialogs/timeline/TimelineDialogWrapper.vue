<script setup lang="ts">
// Components/Dialogs/Timeline/TimelineDialogWrapper.razor – časová osa v panelu (mobil).
import { useMapStore } from '../../../stores/map'
import { t } from '../../../i18n'
import TimelineView from './TimelineView.vue'

const store = useMapStore()
</script>

<template>
  <div v-if="store.windowWidth > 0" class="rz-px-6 rz-pt-12" style="height: 400px">
    <h1 class="rz-pb-4" style="color: var(--Wine-Red)">{{ t('timeline') }}</h1>
    <TimelineView :width="store.windowWidth" mobile-version />
  </div>
</template>
