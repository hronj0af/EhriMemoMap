<script setup lang="ts">
// Components/Buttons/JoinButton.razor – odkaz na formulář pro zapojení do projektu.
import { t } from '../../i18n'
import IconJoin from '../icons/IconJoin.vue'
import IconJoinMobile from '../icons/IconJoinMobile.vue'

withDefaults(defineProps<{ mobileVersion?: boolean }>(), { mobileVersion: false })

const FORM_URL = 'https://docs.google.com/forms/d/15jprT8Sbc-Llt1CfAPrIzU4S8YIDjREixRADGzbA62k/viewform?edit_requested=true'

function redirectToForm() {
  window.location.href = FORM_URL
}
</script>

<template>
  <div v-if="!mobileVersion" class="icon" :title="t('feedbackForm') + '...'" @click="redirectToForm">
    <IconJoin />
  </div>
  <div v-else class="icon rz-mr-2" style="height: 22px" :title="t('joinToProject') + '...'" @click="redirectToForm">
    <IconJoinMobile />
  </div>
</template>
