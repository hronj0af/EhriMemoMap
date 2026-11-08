<script setup lang="ts">
// RadzenRow – flex řádek s 12sloupcovou mřížkou (sloupce viz RzColumn).
import { computed } from 'vue'
import { alignClass, gapValue, type FlexAlign } from './radzen'

const props = defineProps<{
  gap?: string | number
  rowGap?: string | number
  alignItems?: FlexAlign
  justifyContent?: FlexAlign
}>()

const classes = computed(() => [
  'rz-display-flex',
  'rz-row',
  `rz-align-items-${alignClass(props.alignItems)}`,
  `rz-justify-content-${alignClass(props.justifyContent)}`,
])
const style = computed(() => {
  const s: Record<string, string> = {}
  const gap = gapValue(props.gap)
  const rowGap = gapValue(props.rowGap)
  if (gap !== null) s['--rz-gap'] = gap
  if (rowGap !== null) s['--rz-row-gap'] = rowGap
  return s
})
</script>

<template>
  <div :class="classes" :style="style"><slot /></div>
</template>
