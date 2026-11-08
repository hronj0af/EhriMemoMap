<script setup lang="ts">
// Components/Dialogs/Shared/DocumentGallery.razor – náhledy dokumentů, po kliknutí galerie BiggerPicture.
// Skryté odkazy <a> slouží jako položky galerie (obrázky: data-img, videa: href + data-sources).
import { nextTick, onMounted, onUpdated } from 'vue'
import type { Document } from '../../../models/api'
import { documentTitle } from '../../../models/helpers'
import { useI18n } from '../../../i18n'
import { adjustDocumentContainerWidths, initGallery } from '../../../utils/dom'
import { isImageDocument, sourcesJson, uniqueId } from './documents'

const props = withDefaults(defineProps<{ documents?: Document[] | null; showTitle?: boolean }>(), {
  documents: null,
  showTitle: true,
})
const { t, culture } = useI18n()
const guid = uniqueId()

function thumbnail(doc: Document): string | null {
  return doc.url?.[0]?.thumbnailUrl ?? doc.url?.[0]?.url ?? null
}

function showImage(documentId: number, documentIndex: number) {
  // více souborů u některého dokumentu → galerie jen z daného dokumentu, jinak ze všech dokumentů
  if (props.documents?.some((a) => (a.url?.length ?? 0) > 1)) void initGallery(documentId, 0)
  else void initGallery(guid, documentIndex)
}

function adjust() {
  if (props.documents?.length) void nextTick(() => adjustDocumentContainerWidths(guid))
}
onMounted(adjust)
onUpdated(adjust)
</script>

<template>
  <template v-if="documents && documents.length > 0">
    <h3 v-if="showTitle" class="rz-mt-8 rz-mb-4">{{ t('relatedDocuments') }}</h3>
    <div :id="'documents_' + guid" class="rz-mt-8">
      <template v-for="(doc, index) in documents" :key="doc.id">
        <div :id="`doc-container-${guid}-${index}`" style="position: relative; display: inline-block; height: 100px; margin-right: 3px">
          <img
            style="cursor: pointer"
            class="document-photo"
            :alt="documentTitle(doc, culture)"
            :src="thumbnail(doc) ?? undefined"
            @click="showImage(doc.id, index)"
          />
          <img
            v-if="!isImageDocument(doc.type)"
            src="images/icon-play.png"
            style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); width: 48px; height: 48px; opacity: 0.9; pointer-events: none"
          />
        </div>
        <!-- skrytý kontejner s položkami pro BiggerPicture -->
        <div :id="'documents_' + doc.id" style="display: none">
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
                :data-thumb="url.thumbnailUrl ?? thumbnail(doc)"
              ></a>
            </template>
          </template>
        </div>
      </template>
    </div>
  </template>
</template>
