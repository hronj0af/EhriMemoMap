<script setup lang="ts">
// Components/Dialogs/Timeline/Timeline.razor – časová osa (body po čtvrtletích, popisky roků).
// Výpočty pozic stejně jako v C# (celočíselné dělení).
import { computed } from 'vue'
import { useMapStore } from '../../../stores/map'
import { parseApiDate } from '../../../models/helpers'
import { t } from '../../../i18n'
import RzStack from '../../ui/RzStack.vue'
import RzButton from '../../ui/RzButton.vue'
import type { TimelinePointModel } from '../../../models/map'

const props = withDefaults(defineProps<{ width: number; mobileVersion?: boolean }>(), { mobileVersion: false })
const store = useMapStore()

const height = 72
const firstColumnWidth = 100
const margin = 25

const timelineWidth = computed(() => (props.mobileVersion ? props.width : props.width - firstColumnWidth))
const timelineStyle = computed(
  () => `background-color: var(--Grey);position: relative;width:${timelineWidth.value}px;height:${height}px;`,
)

const timeline = computed(() => store.timeline ?? [])
const datedPoints = computed(() => timeline.value.filter((a) => a.from != null && a.to != null))
const allPoint = computed(() => timeline.value.find((a) => a.from == null && a.to == null) ?? null)
const visible = computed(() => datedPoints.value.length > 0)

interface PointView {
  point: TimelinePointModel
  left: number
  step: number
  /** rok, pokud bod začíná nový rok (zobrazí se popisek roku a větší bod) */
  year: number | null
}

const points = computed<PointView[]>(() => {
  const count = datedPoints.value.length
  const step = count > 1 ? Math.trunc((timelineWidth.value - 2 * margin) / (count - 1)) : 0
  let left = margin
  let year: number | null = null
  const result: PointView[] = []
  for (const point of datedPoints.value) {
    const pointYear = parseApiDate(point.from)?.year ?? null
    const newYear = pointYear !== year
    if (newYear) year = pointYear
    result.push({ point, left, step, year: newYear ? year : null })
    left += step
  }
  return result
})

async function onCollectionClick(point: TimelinePointModel | null) {
  await store.selectTimelinePoint(point)
}
</script>

<template>
  <template v-if="visible">
    <RzStack orientation="horizontal" gap="0" align-items="start" class="timeline-inner-wrapper">
      <div v-if="!mobileVersion" class="timeline-firstcolumn" :style="`height:${height - 10}px;width:${width - timelineWidth}px;`">
        <h4 class="rz-mb-2">{{ t('timeline') }}</h4>
        <RzButton
          v-if="allPoint"
          size="xs"
          button-style="secondary"
          variant="outlined"
          :text="t('all')"
          @click="onCollectionClick(allPoint)"
        />
      </div>
      <div class="rz-card rz-variant-filled timeline" :style="timelineStyle">
        <div class="timeline-baseline"></div>
        <template v-for="p in points" :key="p.point.name ?? ''">
          <div class="timeline-point" :style="`left:${p.left - p.step / 2}px;width:${p.step}px`" @click="onCollectionClick(p.point)">
            <div style="position: relative; width: 100%">
              <div :class="p.year !== null ? 'timeline-year-point' : 'timeline-quarter-point'"></div>
            </div>
          </div>
          <div
            :style="(p.year !== null ? 'position:absolute;' : '') + `left:${p.left - 34}px`"
            :class="!p.point.selected ? 'timeline-tooltip' : 'timeline-tooltip-selected'"
            @click="onCollectionClick(p.point)"
          >
            <img src="/images/tooltip.svg" />
            <div class="timeline-tooltip-text">{{ p.point.title }}</div>
          </div>
          <div v-if="p.year !== null" class="timeline-year-label" :style="`left:${p.left - 12}px`">{{ p.year }}</div>
        </template>
      </div>
    </RzStack>
    <div v-if="mobileVersion && allPoint">
      <RzButton
        button-style="primary"
        variant="outlined"
        class="rz-mt-4"
        style="width: 100%"
        :text="t('all')"
        @click="onCollectionClick(allPoint)"
      />
    </div>
  </template>
</template>
