<script setup lang="ts">
// RadzenIcon – ikona z fontu Material Symbols (název ikony jako text, např. "close").
defineProps<{ icon: string }>()
</script>

<template>
  <i class="notranslate rzi">{{ icon }}</i>
</template>
