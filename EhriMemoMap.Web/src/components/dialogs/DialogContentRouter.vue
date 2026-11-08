<script setup lang="ts">
// Components/Dialogs/DialogContentRouter.razor – obsah postranního panelu podle store.dialogType.
import { useMapStore } from '../../stores/map'
import { DialogType } from '../../models/enums'
import DialogButtons from '../buttons/DialogButtons.vue'
import HelpDialog from './help/HelpDialog.vue'
import LayersDialog from './layers/LayersDialog.vue'
import SearchDialog from './search/SearchDialog.vue'
import PlaceDialog from './place/PlaceDialog.vue'
import TimelineDialogWrapper from './timeline/TimelineDialogWrapper.vue'
import StoryMapDialog from './storyMap/StoryMapDialog.vue'
import VictimDialog from './victim/VictimDialog.vue'
import WelcomeDialog from './welcome/WelcomeDialog.vue'

const store = useMapStore()
</script>

<template>
  <DialogButtons />
  <HelpDialog v-if="store.dialogType === DialogType.Help" />
  <LayersDialog v-else-if="store.dialogType === DialogType.Layers" />
  <SearchDialog v-else-if="store.dialogType === DialogType.Search" />
  <PlaceDialog v-else-if="store.dialogType === DialogType.Place" />
  <TimelineDialogWrapper v-else-if="store.dialogType === DialogType.Timeline" />
  <StoryMapDialog v-else-if="store.dialogType === DialogType.StoryMap" />
  <VictimDialog v-else-if="store.dialogType === DialogType.Victim" />
  <WelcomeDialog v-else-if="store.dialogType === DialogType.Welcome" />
</template>
