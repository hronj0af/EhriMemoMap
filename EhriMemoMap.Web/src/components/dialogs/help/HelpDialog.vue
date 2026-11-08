<script setup lang="ts">
// Components/Dialogs/Help/Help.razor
import { useMapStore } from '../../../stores/map'
import { t } from '../../../i18n'

const store = useMapStore()
</script>

<template>
  <div style="padding: 2.5rem 2.5rem 0 2.5rem">
    <h1 class="rz-mb-4" style="color: var(--Wine-Red)">{{ t('help') }}</h1>
    <div v-html="t('helpContent_' + store.city)"></div>
  </div>
</template>
