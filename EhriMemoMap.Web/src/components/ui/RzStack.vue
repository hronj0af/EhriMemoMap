<script setup lang="ts">
// RadzenStack – flex kontejner (výchozí svisle).
import { computed } from 'vue'
import { alignClass, gapValue, type FlexAlign } from './radzen'

const props = withDefaults(
  defineProps<{
    orientation?: 'horizontal' | 'vertical'
    alignItems?: FlexAlign
    justifyContent?: FlexAlign
    gap?: string | number
    wrap?: boolean
  }>(),
  { orientation: 'vertical', wrap: false },
)

const classes = computed(() => [
  'rz-stack',
  'rz-display-flex',
  props.orientation === 'horizontal' ? 'rz-flex-row' : 'rz-flex-column',
  `rz-align-items-${alignClass(props.alignItems)}`,
  `rz-justify-content-${alignClass(props.justifyContent)}`,
])
const style = computed(() => {
  const s: Record<string, string> = { 'flex-wrap': props.wrap ? 'wrap' : 'nowrap' }
  const gap = gapValue(props.gap)
  if (gap !== null) s['--rz-gap'] = gap
  return s
})
</script>

<template>
  <div :class="classes" :style="style"><slot /></div>
</template>
