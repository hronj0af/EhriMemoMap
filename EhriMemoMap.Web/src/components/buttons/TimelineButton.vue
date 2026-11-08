<script setup lang="ts">
// Components/Buttons/TimelineButton.razor – zobrazí se jen pokud má mapa časovou osu (víc než 1 bod).
import { computed } from 'vue'
import { useMapStore } from '../../stores/map'
import { DialogType } from '../../models/enums'
import { t } from '../../i18n'
import RzStack from '../ui/RzStack.vue'
import IconTimeline from '../icons/IconTimeline.vue'
import IconTimelineMobile from '../icons/IconTimelineMobile.vue'

withDefaults(defineProps<{ mobileVersion?: boolean; border?: boolean }>(), { mobileVersion: false, border: false })
const store = useMapStore()

const hasTimeline = computed(() => (store.timeline?.length ?? 0) > 1)

async function showTimeline() {
  await store.setDialog(store.dialogType === DialogType.Timeline ? DialogType.None : DialogType.Timeline)
}
</script>

<template>
  <template v-if="hasTimeline">
    <div
      v-if="!mobileVersion"
      :class="['icon', { 'icon-selected': store.dialogType === DialogType.Timeline }]"
      :title="t('timeline') + '...'"
      @click="showTimeline"
    >
      <IconTimeline />
    </div>
    <RzStack
      v-else
      orientation="horizontal"
      justify-content="center"
      :class="[
        store.dialogType === DialogType.Timeline ? 'mobile-menu-icon-selected' : 'mobile-menu-icon',
        { 'mobile-menu-icon-border': border },
      ]"
      :title="t('timeline') + '...'"
      @click="showTimeline"
    >
      <IconTimelineMobile />
    </RzStack>
  </template>
</template>
