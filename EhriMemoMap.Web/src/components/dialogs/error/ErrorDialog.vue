<script setup lang="ts">
// Components/Dialogs/Error/Error.razor – aplikace mimo provoz (AppState = Shutdown) nebo nenačtené nastavení mapy.
import { useMapStore } from '../../../stores/map'
import { getConfig } from '../../../config'
import { byLang } from '../../../i18n'
import RzRow from '../../ui/RzRow.vue'
import RzColumn from '../../ui/RzColumn.vue'

const store = useMapStore()
const config = getConfig()
</script>

<template>
  <RzRow v-if="!store.isMobileView">
    <RzColumn :size="12" style="background-color: var(--Wine-Red); color: var(--Beige)">
      <div style="padding: 1.5rem">
        <h1 style="padding-bottom: 10px; font-size: 48px">MemoMap je mimo provoz</h1>
        <div style="font-size: 15px; padding-bottom: 10px" v-html="config.messageCzech"></div>
        <h1 style="padding-bottom: 10px; font-size: 48px">MemoMap is down</h1>
        <div style="font-size: 15px; padding-bottom: 10px" v-html="config.messageEnglish"></div>
      </div>
    </RzColumn>
  </RzRow>
  <RzRow v-else>
    <!-- Mobil: zpráva v aktuálním jazyce (Blazor tu omylem zobrazoval hodnotu App.AppState, např. "Shutdown") -->
    <RzColumn :size="12" style="color: var(--Grey)">
      <div style="padding: 1.5rem">
        <h1 style="padding-bottom: 10px; font-size: 48px">MemoMap</h1>
        <div style="font-size: 12px" v-html="byLang(config.messageCzech, config.messageEnglish)"></div>
      </div>
      <div style="padding: 1.5rem"></div>
    </RzColumn>
  </RzRow>
</template>
