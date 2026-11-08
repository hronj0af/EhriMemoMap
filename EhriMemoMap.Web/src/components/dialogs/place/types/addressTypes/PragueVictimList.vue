<script setup lang="ts">
// Components/Dialogs/Place/Types/AddressTypes/PragueVictimList.razor – oběti na pražské adrese.
// Při vybraném bodu časové osy rozdělené na přítomné (transport po datu) a nepřítomné (transport k datu).
// Filtr jmen platí i pro rozdělené skupiny (v Blazoru se uplatňoval jen na zobrazení nadpisu).
import { computed } from 'vue'
import type { VictimShortInfoModel } from '../../../../../models/api'
import { useMapStore } from '../../../../../stores/map'
import { dateKey, formatDate } from '../../../../../models/helpers'
import { t } from '../../../../../i18n'
import RzRow from '../../../../ui/RzRow.vue'
import RzColumn from '../../../../ui/RzColumn.vue'
import VictimShortInfo from '../../../shared/VictimShortInfo.vue'
import { anyMatchesFilter, matchesFilter } from '../victimFilter'

const props = defineProps<{ victims: VictimShortInfoModel[] | null; filteredName?: string }>()
const store = useMapStore()

const visible = computed(() => !!props.victims && props.victims.length > 0 && anyMatchesFilter(props.victims, props.filteredName))
const point = computed(() => store.timelinePoint)

// C# porovnání DateTime? – s null je výsledek false (oběť bez data transportu není v žádné skupině)
const filtered = computed(() => (props.victims ?? []).filter((a) => matchesFilter(a, props.filteredName)))

const present = computed(() =>
  filtered.value.filter((a) => {
    const d = dateKey(a.transportDate)
    return d !== null && point.value !== null && d > point.value
  }),
)
const absent = computed(() =>
  filtered.value.filter((a) => {
    const d = dateKey(a.transportDate)
    return d !== null && point.value !== null && d <= point.value
  }),
)
</script>

<template>
  <RzRow class="rz-mt-8" gap="1rem" row-gap="2rem" justify-content="start">
    <template v-if="visible">
      <template v-if="point != null">
        <template v-if="present.length > 0">
          <RzColumn :size="12">
            <h4>{{ t('present') }} ({{ formatDate(point) }})</h4>
          </RzColumn>
          <VictimShortInfo v-for="(victim, i) in present" :key="'p' + i" :victim="victim" />
        </template>
        <template v-if="absent.length > 0">
          <RzColumn :size="12">
            <h4>{{ t('absent') }} ({{ formatDate(point) }})</h4>
          </RzColumn>
          <VictimShortInfo v-for="(victim, i) in absent" :key="'a' + i" :victim="victim" />
        </template>
      </template>
      <template v-else>
        <VictimShortInfo v-for="(victim, i) in filtered" :key="i" :victim="victim" />
      </template>
    </template>
  </RzRow>
</template>
