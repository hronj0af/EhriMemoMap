<script setup lang="ts">
// Components/Dialogs/Timeline/TimelineDialogWrapper.razor – časová osa v panelu (mobil).
// Šířka osy podle skutečné šířky obsahu panelu (Blazor používal šířku celého okna, takže osa
// přesahovala okraje panelu a poslední rok byl uříznutý). Přepočítá se při změně velikosti okna.
import { nextTick, onMounted, ref, watch } from 'vue'
import { useMapStore } from '../../../stores/map'
import { t } from '../../../i18n'
import TimelineView from './TimelineView.vue'

const store = useMapStore()
const container = ref<HTMLElement | null>(null)
const width = ref(0)

function measure() {
  if (container.value) width.value = container.value.clientWidth
}

onMounted(measure)
watch(
  () => store.windowWidth,
  () => void nextTick(measure),
)
</script>

<template>
  <div class="rz-px-6 rz-pt-12" style="height: 400px">
    <h1 class="rz-pb-4" style="color: var(--Wine-Red)">{{ t('timeline') }}</h1>
    <div ref="container">
      <TimelineView v-if="width > 0" :key="width" :width="width" mobile-version />
    </div>
  </div>
</template>
