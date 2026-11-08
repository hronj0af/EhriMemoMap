<script setup lang="ts">
// RadzenButton – tlačítko se stejnými třídami jako Radzen (vzhled z material-base.css).
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    text?: string
    icon?: string
    size?: 'xs' | 'sm' | 'md' | 'lg'
    variant?: 'filled' | 'flat' | 'outlined' | 'text'
    buttonStyle?: 'primary' | 'secondary' | 'light' | 'dark' | 'base' | 'info' | 'success' | 'warning' | 'danger'
    disabled?: boolean
  }>(),
  { size: 'md', variant: 'filled', buttonStyle: 'primary', disabled: false },
)
const emit = defineEmits<{ click: [event: MouseEvent] }>()
const slots = defineSlots<{ default?(): unknown }>()

const iconOnly = computed(() => !!props.icon && !props.text && !slots.default)
const classes = computed(() => [
  'rz-button',
  `rz-button-${props.size}`,
  `rz-variant-${props.variant}`,
  `rz-${props.buttonStyle}`,
  'rz-shade-default',
  { 'rz-button-icon-only': iconOnly.value, 'rz-state-disabled': props.disabled },
])
</script>

<template>
  <button type="button" :class="classes" :disabled="disabled" @click="emit('click', $event)">
    <span class="rz-button-box">
      <i v-if="icon" class="notranslate rz-button-icon-left rzi">{{ icon }}</i>
      <span v-if="text" class="rz-button-text">{{ text }}</span>
      <slot />
    </span>
  </button>
</template>
