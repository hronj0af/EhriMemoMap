<script setup lang="ts">
// Components/Dialogs/StoryMap/StoryMap.razor – seznam příběhů, nebo detail vybraného příběhu.
import { useMapStore } from '../../../stores/map'
import { useI18n } from '../../../i18n'
import RzRow from '../../ui/RzRow.vue'
import RzColumn from '../../ui/RzColumn.vue'
import StoryMapTimeline from './StoryMapTimeline.vue'

const store = useMapStore()
const { t, culture } = useI18n()
</script>

<template>
  <div id="card-narrative" style="padding: 2.5rem 2.5rem 0 2.5rem">
    <template v-if="store.allNarrativeMaps != null">
      <template v-if="store.dialogParameters.id == null">
        <h1 class="rz-mb-8" style="color: var(--Wine-Red)">{{ t('storyMaps') }}</h1>
        <div v-for="map in store.allNarrativeMaps" :key="map.id">
          <h4 class="rz-mb-6" style="cursor: pointer" @click="store.showNarrativeMap(map.id)">
            {{ culture === 'cs-CZ' ? map.labelCs : map.labelEn }}
          </h4>
        </div>
      </template>
      <RzRow
        v-else-if="store.allNarrativeMaps.length > 1"
        align-items="center"
        class="rz-mb-4"
        style="cursor: pointer"
        @click="store.showAllStoryMaps()"
      >
        <RzColumn :size="11">
          <h4>{{ t('backToOtherStoryMaps') }}</h4>
        </RzColumn>
        <RzColumn :size="1">
          <img src="images/storymap-icon.png" style="height: 30px" />
        </RzColumn>
      </RzRow>

      <template v-if="store.dialogParameters.id != null || store.allNarrativeMaps.length === 1">
        <RzRow>
          <RzColumn :size="12">
            <h1 class="rz-mb-6" style="color: var(--Wine-Red)">
              {{ culture === 'cs-CZ' ? store.narrativeMap?.labelCs : store.narrativeMap?.labelEn }}
            </h1>
          </RzColumn>
        </RzRow>
        <StoryMapTimeline />
      </template>
    </template>
  </div>
</template>
