<script setup lang="ts">
// Components/Buttons/HelpButton.razor
import { useMapStore } from '../../stores/map'
import { DialogType } from '../../models/enums'
import { t } from '../../i18n'
import IconHelp from '../icons/IconHelp.vue'
import IconHelpMobile from '../icons/IconHelpMobile.vue'

withDefaults(defineProps<{ mobileVersion?: boolean }>(), { mobileVersion: false })
const store = useMapStore()

async function showHelp() {
  await store.setDialog(store.dialogType === DialogType.Help ? DialogType.None : DialogType.Help)
}
</script>

<template>
  <div
    v-if="!mobileVersion"
    :class="['icon', { 'icon-selected': store.dialogType === DialogType.Help }]"
    :title="t('help') + '...'"
    @click="showHelp"
  >
    <IconHelp />
  </div>
  <div v-else class="icon" style="height: 22px" :title="t('help') + '...'" @click="showHelp">
    <IconHelpMobile />
  </div>
</template>
