<script setup lang="ts">
// Components/ButtonWrappers/MobileTopBarWrapper.razor – horní lišta na mobilu.
import { useRouter } from 'vue-router'
import { useMapStore } from '../../stores/map'
import { DialogType, MapState } from '../../models/enums'
import RzRow from '../ui/RzRow.vue'
import RzColumn from '../ui/RzColumn.vue'
import RzStack from '../ui/RzStack.vue'
import JoinButton from '../buttons/JoinButton.vue'
import HelpButton from '../buttons/HelpButton.vue'
import LanguageButton from '../buttons/LanguageButton.vue'

const store = useMapStore()
const router = useRouter()

async function showHome() {
  await store.setDialog(DialogType.None)
  await router.push('/')
}
</script>

<template>
  <div
    v-if="store.mapState === MapState.Rendered && store.isMobileView"
    style="background-color: var(--Wine-Red); color: var(--Beige); position: absolute; top: 0px; left: 0px; width: 100%; z-index: 1001"
  >
    <RzRow align-items="center" justify-content="space-between" style="padding: 10px">
      <RzColumn :size="6">
        <h2 style="cursor: pointer" @click="showHome">MemoMap</h2>
      </RzColumn>
      <RzColumn :size="6">
        <RzStack orientation="horizontal" align-items="center" justify-content="end">
          <JoinButton mobile-version />
          <HelpButton mobile-version />
          <LanguageButton mobile-version />
        </RzStack>
      </RzColumn>
    </RzRow>
  </div>
</template>
