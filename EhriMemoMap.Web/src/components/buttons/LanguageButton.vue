<script setup lang="ts">
// Components/Buttons/LanguageButton.razor – přepnutí češtiny/angličtiny (s reloadem stránky jako v Blazoru).
import { computed } from 'vue'
import { useI18n } from '../../i18n'
import RzButton from '../ui/RzButton.vue'
import IconLanguageBackground from '../icons/IconLanguageBackground.vue'

withDefaults(defineProps<{ mobileVersion?: boolean }>(), { mobileVersion: false })
const { t, culture, setCulture } = useI18n()

const otherLanguage = computed(() => (culture.value === 'cs-CZ' ? 'EN' : 'CS'))
const otherLanguageTitle = computed(() => (culture.value === 'cs-CZ' ? 'english' : 'czech'))

function changeLanguage() {
  setCulture(culture.value === 'cs-CZ' ? 'en-US' : 'cs-CZ')
}
</script>

<template>
  <RzButton v-if="mobileVersion" id="btn-language-topbar" size="xs" variant="flat" @click="changeLanguage">
    <b>{{ otherLanguage }}</b>
  </RzButton>
  <div v-else class="language-btn-wrapper" :title="t(otherLanguageTitle)" @click="changeLanguage">
    <IconLanguageBackground />
    <div class="language-text">
      <h3><b>{{ otherLanguage }}</b></h3>
    </div>
  </div>
</template>
