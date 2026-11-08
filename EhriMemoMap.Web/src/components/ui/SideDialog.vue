<script setup lang="ts">
// Postranní panel – náhrada Radzen DialogService.OpenSide<DialogContentRouter>.
// Vpravo na desktopu (šířka 33 %), dole na mobilu (výška 50 %). Nastavení určí store při otevření
// (sideDialogOptions, viz MapService.GetDialogOptions). Režim "přes celou obrazovku" přebírá chování
// site.ts setDialogFullScreen (panel 100 % × 100 %).
import { computed } from 'vue'
import { useMapStore } from '../../stores/map'
import { DialogType } from '../../models/enums'
import DialogContentRouter from '../dialogs/DialogContentRouter.vue'

const store = useMapStore()

const visible = computed(() => store.dialogType !== DialogType.None && store.sideDialogOptions !== null)

const classes = computed(() => {
  const o = store.sideDialogOptions
  return ['rz-dialog-side', `rz-dialog-side-position-${o?.position ?? 'right'}`, o?.cssClass ?? '', 'rz-open']
})

const style = computed(() => {
  const o = store.sideDialogOptions
  if (!o) return ''
  const parts: string[] = []
  if (store.isDialogFullScreen) parts.push('width:100%', 'height:100%')
  else {
    if (o.width) parts.push(`width:${o.width}`)
    if (o.height) parts.push(`height:${o.height}`)
  }
  if (o.style) parts.push(o.style)
  return parts.join(';')
})
</script>

<template>
  <aside v-if="visible" :class="classes" :style="style">
    <div class="rz-dialog-side-content">
      <DialogContentRouter />
    </div>
  </aside>
</template>
