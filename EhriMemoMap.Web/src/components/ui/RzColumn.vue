<script setup lang="ts">
// RadzenColumn – sloupec 12sloupcové mřížky (size 1–12, responzivní sizeSm/Md/Lg, offset).
import { computed } from 'vue'

const props = defineProps<{
  size?: number | null
  sizeSm?: number
  sizeMd?: number
  sizeLg?: number
  offset?: number
}>()

const classes = computed(() => {
  const c = ['rz-col']
  if (props.size) c.push(`rz-col-${props.size}`)
  if (props.sizeSm) c.push(`rz-col-sm-${props.sizeSm}`)
  if (props.sizeMd) c.push(`rz-col-md-${props.sizeMd}`)
  if (props.sizeLg) c.push(`rz-col-lg-${props.sizeLg}`)
  if (props.offset) c.push(`rz-offset-${props.offset}`)
  return c
})
</script>

<template>
  <div :class="classes"><slot /></div>
</template>
