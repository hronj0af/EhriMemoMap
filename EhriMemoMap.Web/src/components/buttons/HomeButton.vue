<script setup lang="ts">
// Components/Buttons/HomeButton.razor
import { useRouter } from 'vue-router'
import { useMapStore } from '../../stores/map'
import { DialogType } from '../../models/enums'
import { t } from '../../i18n'
import IconHome from '../icons/IconHome.vue'

const store = useMapStore()
const router = useRouter()

async function showHome() {
  await store.setDialog(DialogType.None)
  await router.push('/')
}
</script>

<template>
  <div class="icon" :title="t('backToHomepage') + '...'" @click="showHome">
    <IconHome />
  </div>
</template>
