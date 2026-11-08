<script setup lang="ts">
// RadzenTabs – záložky; obsah záložky vykreslí výchozí slot s indexem záložky.
// Jako Radzen s TabRenderMode.Client jsou vykreslené všechny panely a neaktivní se jen skryjí.
import { ref } from 'vue'

defineProps<{ titles: string[] }>()
const selected = ref(0)
</script>

<template>
  <div class="rz-tabview rz-tabview-top">
    <ul class="rz-tabview-nav" role="tablist">
      <li
        v-for="(title, index) in titles"
        :key="index"
        role="presentation"
        :class="{ 'rz-tabview-selected': index === selected }"
      >
        <a role="tab" href="#" :aria-selected="index === selected" @click.prevent="selected = index">
          <span class="rz-tabview-title">{{ title }}</span>
        </a>
      </li>
    </ul>
    <div class="rz-tabview-panels">
      <div v-for="(title, index) in titles" v-show="index === selected" :key="title + index" class="rz-tabview-panel" role="tabpanel">
        <slot :index="index" />
      </div>
    </div>
  </div>
</template>
