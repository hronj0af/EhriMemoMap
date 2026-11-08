<script setup lang="ts">
// Components/Dialogs/StoryMap/StoryMapTimeline.razor – zastávky příběhu jako svislá časová osa,
// rozbalená zastávka ukazuje adresu, datum, popis, dokumenty a rozbalovací sekce.
import { computed } from 'vue'
import { useMapStore } from '../../../stores/map'
import { nl2br } from '../../../models/helpers'
import { useI18n } from '../../../i18n'
import RzRow from '../../ui/RzRow.vue'
import RzColumn from '../../ui/RzColumn.vue'
import DocumentGallery from '../shared/DocumentGallery.vue'

const store = useMapStore()
const { culture } = useI18n()

const stops = computed(() => {
  const nm = store.narrativeMap
  if (!nm) return []
  return (nm.stops ?? []).filter((a) => a.places?.some((b) => b.narrativeMapId === nm.id) ?? false)
})

function isStopExpanded(stopId: number) {
  return store.expandedStopId === stopId
}
function isSectionExpanded(key: string) {
  return store.expandedSections.has(key)
}
function toggleSection(key: string) {
  if (store.expandedSections.has(key)) store.expandedSections.delete(key)
  else store.expandedSections.add(key)
}
function cs<T>(csValue: T, enValue: T): T {
  return culture.value === 'cs-CZ' ? csValue : enValue
}
</script>

<template>
  <div class="storymap-timeline-container">
    <template v-for="(stop, i) in stops" :key="stop.id">
      <a :id="'narrative-stop-' + stop.id" />
      <div :class="['storymap-timeline-item', { 'storymap-timeline-item-last': i === stops.length - 1 }]">
        <div
          :class="[
            'storymap-timeline-line',
            i === stops.length - 1 && isStopExpanded(stop.id)
              ? 'storymap-timeline-line-last-expanded'
              : i === stops.length - 1
                ? 'storymap-timeline-line-last-collapsed'
                : '',
          ]"
        ></div>
        <div :class="isStopExpanded(stop.id) ? 'storymap-timeline-dot-big' : 'storymap-timeline-dot-small'">
          <img
            :src="isStopExpanded(stop.id) ? 'images/story-big-dot.png' : 'images/story-small-dot.png'"
            :alt="isStopExpanded(stop.id) ? 'Rozbaleno' : 'Sbaleno'"
          />
        </div>
        <div class="storymap-timeline-content">
          <div style="cursor: pointer" @click="store.openStoryMapTimelineLabel(stop.id, store.narrativeMap?.id ?? null)">
            <RzRow align-items="center">
              <RzColumn :size="12">
                <div style="display: flex; align-items: center; gap: 8px">
                  <h3 :style="(isStopExpanded(stop.id) ? 'color:var(--Wine-Red)' : '') + '; margin: 0;'">
                    {{ cs(stop.labelCs, stop.labelEn) }}
                  </h3>
                  <img
                    :src="isStopExpanded(stop.id) ? 'images/arrow-down.png' : 'images/arrow-right.png'"
                    :alt="isStopExpanded(stop.id) ? 'Sbalit' : 'Rozbalit'"
                  />
                </div>
              </RzColumn>
            </RzRow>
          </div>
          <template v-if="isStopExpanded(stop.id)">
            <h4 class="rz-mt-2">{{ cs(stop.places?.[0]?.addressCs, stop.places?.[0]?.addressEn) }}</h4>
            <div class="rz-mt-2">
              <i>{{ cs(stop.dateCs, stop.dateEn) }}</i>
            </div>
            <div class="rz-mt-4" v-html="nl2br(cs(stop.descriptionCs, stop.descriptionEn))"></div>
            <DocumentGallery v-if="stop.documents && stop.documents.length > 0" :show-title="false" :documents="stop.documents" />
            <div v-if="(cs(stop.descriptionSectionsCs, stop.descriptionSectionsEn) ?? []).length > 0" class="rz-mt-4">
              <div
                v-for="(section, sectionIndex) in cs(stop.descriptionSectionsCs, stop.descriptionSectionsEn) ?? []"
                :key="sectionIndex"
                class="rz-mb-4"
              >
                <div style="cursor: pointer" @click="toggleSection(`${stop.id}_${sectionIndex}`)">
                  <div style="display: flex; align-items: center; gap: 8px">
                    <h4 style="color: var(--Wine-Red); margin: 0">{{ section.title }}</h4>
                    <img
                      :src="isSectionExpanded(`${stop.id}_${sectionIndex}`) ? 'images/arrow-down.png' : 'images/arrow-right.png'"
                      :alt="isSectionExpanded(`${stop.id}_${sectionIndex}`) ? 'Sbalit' : 'Rozbalit'"
                    />
                  </div>
                </div>
                <div v-if="isSectionExpanded(`${stop.id}_${sectionIndex}`)" class="rz-mt-2" v-html="nl2br(section.text)"></div>
              </div>
            </div>
          </template>
        </div>
      </div>
    </template>
  </div>
</template>
