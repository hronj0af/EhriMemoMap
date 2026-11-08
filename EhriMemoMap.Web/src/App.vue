<script setup lang="ts">
// Kořenová komponenta – náhrada App.razor + Layout/MainLayout.razor.
// Postranní panel a modální dialogy (RadzenDialog) vykresluje MapPage – načítají se až s mapou.
</script>

<template>
  <div class="page">
    <main>
      <!-- klíč podle cesty: přechod mezi městy (/prague → /ricany) vytvoří mapu znovu -->
      <RouterView v-slot="{ Component, route }">
        <component :is="Component" :key="route.path" />
      </RouterView>
    </main>
  </div>
</template>

<style scoped>
/* Layout/MainLayout.razor.css (CSS isolation) – jen pravidla, která se v aplikaci uplatní */
.page {
  position: relative;
  display: flex;
  flex-direction: column;
}

main {
  flex: 1;
}

@media (min-width: 641px) {
  .page {
    flex-direction: row;
  }
}
</style>
