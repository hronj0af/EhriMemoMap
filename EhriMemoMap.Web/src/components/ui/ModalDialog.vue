<script setup lang="ts">
// Modální dialog – náhrada Radzen DialogService.Open<Welcome/Error> (šířka 720 px, bez titulku).
// Uvítání se zavře i kliknutím mimo dialog (CloseDialogOnOverlayClick), chybová hláška ne.
import { computed } from 'vue'
import { useMapStore } from '../../stores/map'
import { ModalType } from '../../models/enums'
import WelcomeDialog from '../dialogs/welcome/WelcomeDialog.vue'
import ErrorDialog from '../dialogs/error/ErrorDialog.vue'

const store = useMapStore()
const visible = computed(() => store.modal !== ModalType.None)

function onOverlayClick() {
  if (store.modal === ModalType.Welcome) store.modal = ModalType.None
}
</script>

<template>
  <template v-if="visible">
    <div class="rz-dialog-wrapper" @click.self="onOverlayClick">
      <div class="rz-dialog" role="dialog" aria-modal="true" style="width: 720px; max-width: 100%">
        <div class="rz-dialog-content">
          <WelcomeDialog v-if="store.modal === ModalType.Welcome" />
          <ErrorDialog v-else-if="store.modal === ModalType.Error" />
        </div>
      </div>
    </div>
    <div class="rz-dialog-mask" @click="onOverlayClick"></div>
  </template>
</template>
