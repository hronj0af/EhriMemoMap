<script setup lang="ts">
// Kořenová komponenta – náhrada App.razor + Layout/MainLayout.razor.
// Styly z MainLayout.razor.css se nepřebírají – Blazor je nenačítal (odkaz na EhriMemoMap.styles.css
// byl v index.html zakomentovaný) a .page { position: relative } by posunulo ukotvení lišty tlačítek.
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

