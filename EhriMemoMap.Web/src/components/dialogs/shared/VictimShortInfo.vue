<script setup lang="ts">
// Components/Dialogs/Shared/VictimShortInfo.razor – karta oběti s fotkou.
// Oběť bez detailu (longInfo = false) odkazuje na holocaust.cz, s detailem otevře panel oběti.
import { computed } from 'vue'
import type { VictimShortInfoModel } from '../../../models/api'
import { useMapStore } from '../../../stores/map'
import { documentTitle, victimLabelForPicture } from '../../../models/helpers'
import { useI18n } from '../../../i18n'
import { initGallery } from '../../../utils/dom'
import { isImageDocument, sourcesJson, uniqueId } from './documents'
import RzColumn from '../../ui/RzColumn.vue'
import RzStack from '../../ui/RzStack.vue'

const props = withDefaults(defineProps<{ victim: VictimShortInfoModel | null; small?: boolean; relative?: boolean }>(), {
  small: false,
  relative: false,
})
const store = useMapStore()
const { t, culture } = useI18n()

const victimDocGuid = uniqueId('vsi')
const cardCss = computed(() => (props.small ? 'victim-card-small' : 'victim-card') + ' rz-mb-4')
const photoCss = computed(() => (props.small ? 'victim-photo-small' : 'victim-photo') + ' rz-mb-4')
const face = computed(() => (props.small ? 'images/face-small.png' : 'images/face.png'))
const columnSize = computed(() => (props.small ? (store.isMobileView ? 4 : 3) : store.isMobileView ? 6 : 4))

function hasPhoto(photo: string | null): photo is string {
  return !!photo && photo !== 'NULL' && photo !== 'null'
}

const storyMapTitle = computed(() => (props.victim?.narrativeMapId != null ? t('storyMapExistsForPerson') : ''))

async function onVictimClick() {
  if (!props.victim?.longInfo) return
  await store.showVictim(props.victim.id)
}

function showDocuments() {
  if (!props.victim?.documents?.length) return
  void initGallery(`vsi_${victimDocGuid}`, 0)
}
</script>

<template>
  <RzColumn v-if="victim" :size="columnSize">
    <a
      v-if="!victim.longInfo"
      :href="(culture === 'cs-CZ' ? victim.detailsCs : victim.detailsEn) ?? undefined"
      target="_blank"
      style="text-decoration: none; color: black"
    >
      <div :class="['rz-card', 'rz-variant-filled', cardCss]">
        <RzStack orientation="vertical" align-items="start" gap="5px">
          <div style="position: relative; width: 100%">
            <img :class="photoCss" :src="hasPhoto(victim.photo) ? victim.photo : 'images/face.png'" />
            <template v-if="victim.documents && victim.documents.length > 0">
              <img
                :title="t('relatedDocuments')"
                width="25"
                style="z-index: 99999"
                class="document-forward-icon"
                src="/images/photo-icon.png"
                @click.stop.prevent="showDocuments"
              />
              <div :id="'documents_vsi_' + victimDocGuid" style="display: none">
                <template v-for="doc in victim.documents" :key="doc.id">
                  <template v-if="doc.url">
                    <template v-if="isImageDocument(doc.type)">
                      <a
                        v-for="(url, i) in doc.url"
                        :key="i"
                        :data-img="url.url"
                        :data-caption="documentTitle(doc, culture)"
                        :data-alt="documentTitle(doc, culture)"
                      ></a>
                    </template>
                    <template v-else>
                      <a
                        v-for="(url, i) in doc.url"
                        :key="i"
                        :href="url.url ?? undefined"
                        :data-sources="sourcesJson(url)"
                        :data-thumb="url.thumbnailUrl ?? doc.url[0]?.thumbnailUrl ?? doc.url[0]?.url"
                      ></a>
                    </template>
                  </template>
                </template>
              </div>
            </template>
          </div>
          <div class="victim-label"><small v-html="victimLabelForPicture(victim)"></small></div>
        </RzStack>
      </div>
    </a>
    <div v-else :class="['rz-card', 'rz-variant-filled', cardCss]" style="cursor: pointer" @click="onVictimClick">
      <div style="position: relative; width: 100%">
        <img :title="storyMapTitle" :class="photoCss" :src="hasPhoto(victim.photo) ? victim.photo : face" />
        <img v-if="victim.narrativeMapId != null" :title="storyMapTitle" width="25" class="document-forward-icon" src="/images/storymap-icon.png" />
      </div>
      <div class="victim-label">
        <div>
          <small>
            <b v-if="relative">{{ victim.name }}</b>
            <span v-else v-html="victimLabelForPicture(victim)"></span>
          </small>
        </div>
        <div v-if="victim.relationshipToPersonCs">
          <small>{{ culture === 'cs-CZ' ? victim.relationshipToPersonCs : victim.relationshipToPersonEn }}</small>
        </div>
      </div>
    </div>
  </RzColumn>
</template>
