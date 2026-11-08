<script setup lang="ts">
// Components/Dialogs/Place/Types/AddressTypes/PacovVictimList.razor – obyvatelé adresy v Pacově
// + rozbalovací seznamy obyvatel k 13. 5. 1939 (typ vztahu 24) a k 13. 11. 1942 (typ 26).
import { computed, ref } from 'vue'
import type { VictimShortInfoModel } from '../../../../../models/api'
import { distinctVictims } from '../../../../../models/helpers'
import { t } from '../../../../../i18n'
import RzRow from '../../../../ui/RzRow.vue'
import RzColumn from '../../../../ui/RzColumn.vue'
import VictimShortInfo from '../../../shared/VictimShortInfo.vue'
import { anyMatchesFilter, matchesFilter } from '../victimFilter'

const props = defineProps<{ victims: VictimShortInfoModel[] | null; filteredName?: string }>()
const residents1939Open = ref(false)
const residents1942Open = ref(false)

const all = computed(() => props.victims ?? [])
const visible = computed(() => all.value.length > 0 && anyMatchesFilter(all.value, props.filteredName))
const hasRelationship = computed(() => all.value.some((a) => a.relationshipToAddressType != null))
const filtered = computed(() => distinctVictims(all.value.filter((a) => matchesFilter(a, props.filteredName))))
const residents1939 = computed(() => all.value.filter((a) => a.relationshipToAddressType === 24))
const residents1942 = computed(() => all.value.filter((a) => a.relationshipToAddressType === 26))
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

      <template v-if="residents1939.length > 0">
        <RzColumn :size="12">
          <br />
          <div style="cursor: pointer" @click="residents1939Open = !residents1939Open">
            <h4>
              {{ t('residents_1939-05-13') }}
              &nbsp;&nbsp;
              <img v-if="!residents1939Open" style="height: 17px; cursor: pointer" src="images/arrow-right.png" :title="t('show') + '...'" />
              <img v-else style="width: 17px; cursor: pointer" src="images/arrow-down.png" :title="t('hide') + '...'" />
            </h4>
          </div>
          <hr />
        </RzColumn>
        <template v-if="residents1939Open">
          <VictimShortInfo v-for="(victim, i) in residents1939" :key="'r39' + i" :victim="victim" />
        </template>
      </template>

      <template v-if="residents1942.length > 0">
        <RzColumn :size="12">
          <br />
          <div style="cursor: pointer" @click="residents1942Open = !residents1942Open">
            <h4>
              {{ t('residents_1942-11-13') }}
              &nbsp;&nbsp;
              <img v-if="!residents1942Open" style="height: 17px; cursor: pointer" src="images/arrow-right.png" :title="t('show') + '...'" />
              <img v-else style="width: 17px; cursor: pointer" src="images/arrow-down.png" :title="t('hide') + '...'" />
            </h4>
          </div>
          <hr />
        </RzColumn>
        <template v-if="residents1942Open">
          <VictimShortInfo v-for="(victim, i) in residents1942" :key="'r42' + i" :victim="victim" />
        </template>
      </template>
    </template>
  </RzRow>
</template>
