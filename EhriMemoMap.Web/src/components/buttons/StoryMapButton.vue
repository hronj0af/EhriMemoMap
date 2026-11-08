<script setup lang="ts">
// Components/Buttons/StoryMapButton.razor
import { useMapStore } from '../../stores/map'
import { DialogType } from '../../models/enums'
import { t } from '../../i18n'
import RzStack from '../ui/RzStack.vue'
import IconStoryMap from '../icons/IconStoryMap.vue'
import IconStoryMapMobile from '../icons/IconStoryMapMobile.vue'

withDefaults(defineProps<{ mobileVersion?: boolean; border?: boolean }>(), { mobileVersion: false, border: false })
const store = useMapStore()
</script>

<template>
  <div
    v-if="!mobileVersion"
    :class="['icon', { 'icon-selected': store.dialogType === DialogType.StoryMap }]"
    :title="t('storyMaps') + '...'"
    @click="store.switchStoryMap()"
  >
    <IconStoryMap />
  </div>
  <RzStack
    v-else
    orientation="horizontal"
    justify-content="center"
    :class="[
      store.dialogType === DialogType.StoryMap ? 'mobile-menu-icon-selected' : 'mobile-menu-icon',
      { 'mobile-menu-icon-border': border },
    ]"
    :title="t('storyMaps') + '...'"
    @click="store.switchStoryMap()"
  >
    <IconStoryMapMobile />
  </RzStack>
</template>
