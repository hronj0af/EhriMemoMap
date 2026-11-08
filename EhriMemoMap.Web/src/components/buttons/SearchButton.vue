<script setup lang="ts">
// Components/Buttons/SearchButton.razor
import { useMapStore } from '../../stores/map'
import { DialogType, MapType } from '../../models/enums'
import { t } from '../../i18n'
import RzStack from '../ui/RzStack.vue'
import IconSearch from '../icons/IconSearch.vue'
import IconSearchMobile from '../icons/IconSearchMobile.vue'

const props = withDefaults(defineProps<{ mobileVersion?: boolean; border?: boolean; disabled?: boolean }>(), {
  mobileVersion: false,
  border: false,
  disabled: false,
})
const store = useMapStore()

async function showSearch() {
  if (props.disabled) return
  await store.setDialog(store.dialogType === DialogType.Search ? DialogType.None : DialogType.Search)
  if (store.mapType !== MapType.Normal) await store.setMapType(MapType.Normal)
}
</script>

<template>
  <div
    v-if="!mobileVersion"
    :class="['icon', { 'icon-selected': store.dialogType === DialogType.Search, 'icon-disabled': disabled }]"
    :title="disabled ? '' : t('search') + '...'"
    @click="showSearch"
  >
    <IconSearch :style="disabled ? 'opacity: 0.4;' : ''" />
  </div>
  <!-- Pozn.: Blazor zde měl třídu "border" (ne "mobile-menu-icon-border") a title "layers" – zachováno. -->
  <RzStack
    v-else
    orientation="horizontal"
    justify-content="center"
    :class="[
      store.dialogType === DialogType.Search ? 'mobile-menu-icon-selected' : 'mobile-menu-icon',
      { border: border, 'mobile-menu-icon-disabled': disabled },
    ]"
    :title="disabled ? '' : t('layers') + '...'"
    @click="showSearch"
  >
    <IconSearchMobile :style="disabled ? 'opacity: 0.4;' : ''" />
  </RzStack>
</template>
