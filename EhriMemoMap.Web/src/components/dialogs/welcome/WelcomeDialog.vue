<script setup lang="ts">
// Components/Dialogs/Welcome/Welcome.razor – na desktopu jako modální dialog, na mobilu v panelu.
import { useMapStore } from '../../../stores/map'
import { DialogType, ModalType } from '../../../models/enums'
import { t } from '../../../i18n'
import RzRow from '../../ui/RzRow.vue'
import RzColumn from '../../ui/RzColumn.vue'
import RzButton from '../../ui/RzButton.vue'
import StatisticsTable from './StatisticsTable.vue'

const store = useMapStore()

async function onClose() {
  store.modal = ModalType.None
  await store.setDialog(DialogType.None)
}
</script>

<template>
  <RzRow v-if="!store.isMobileView" gap="0px">
    <RzColumn :size="6" style="background-color: var(--Wine-Red); color: var(--Beige)">
      <div style="padding: 1.5rem">
        <h1 style="padding-bottom: 10px; font-size: 48px">MemoMap</h1>
        <div style="font-size: 15px; padding-bottom: 10px" v-html="t('welcomeText_' + store.city)"></div>
        <RzButton style="text-transform: none" button-style="dark" variant="outlined" :text="t('goToMap')" @click="onClose" />
      </div>
    </RzColumn>
    <RzColumn :size="6" style="color: var(--Grey)">
      <div style="padding: 1.5rem">
        <h1 style="padding-bottom: 10px; padding-top: 15px">{{ t('city_' + store.city) }}</h1>
        <StatisticsTable />
        <div style="font-size: 12px; color: var(--Wine-Red)">{{ t('useNavigation') }}</div>
      </div>
    </RzColumn>
  </RzRow>
  <RzRow v-else>
    <RzColumn :size="12" style="color: var(--Grey)">
      <div style="padding: 1.5rem">
        <h1 style="padding-bottom: 10px; font-size: 48px">MemoMap</h1>
        <div style="font-size: 12px" v-html="t('welcomeText_' + store.city)"></div>
      </div>
      <div style="padding: 1.5rem">
        <StatisticsTable />
        <RzButton class="rz-mt-4" button-style="primary" variant="outlined" :text="t('goToMap')" @click="onClose" />
      </div>
    </RzColumn>
  </RzRow>
</template>
