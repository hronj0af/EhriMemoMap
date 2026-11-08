<script setup lang="ts">
// Components/Dialogs/Place/Types/AddressTypes/RicanyVictimList.razor – obyvatelé adresy v Říčanech (abecedně).
import { computed } from 'vue'
import type { VictimShortInfoModel } from '../../../../../models/api'
import { distinctVictims } from '../../../../../models/helpers'
import { t } from '../../../../../i18n'
import RzRow from '../../../../ui/RzRow.vue'
import RzColumn from '../../../../ui/RzColumn.vue'
import VictimShortInfo from '../../../shared/VictimShortInfo.vue'
import { anyMatchesFilter, matchesFilter } from '../victimFilter'

const props = defineProps<{ victims: VictimShortInfoModel[] | null; filteredName?: string }>()

const all = computed(() => props.victims ?? [])
const visible = computed(() => all.value.length > 0 && anyMatchesFilter(all.value, props.filteredName))
const hasRelationship = computed(() => all.value.some((a) => a.relationshipToAddressType != null))
const filtered = computed(() =>
  distinctVictims(all.value.filter((a) => matchesFilter(a, props.filteredName))).sort((a, b) =>
    (a.label ?? '').localeCompare(b.label ?? '', 'cs'),
  ),
)
</script>

<template>
  <RzRow class="rz-mt-8" gap="1rem" row-gap="2rem" justify-content="start">
    <template v-if="visible">
      <RzColumn v-if="hasRelationship" :size="12">
        <h4>
          {{ t('allResidents') }}
          <span class="tooltip">
            <img src="/images/help-small.svg" class="icon-help" />
            <span class="tooltiptext tooltiptext-top">{{ t('allResidents_help') }}</span>
          </span>
        </h4>
      </RzColumn>
      <VictimShortInfo v-for="(victim, i) in filtered" :key="i" :victim="victim" />
    </template>
  </RzRow>
</template>
