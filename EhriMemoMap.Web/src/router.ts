import { createRouter, createWebHistory } from 'vue-router'
import HomePage from './pages/HomePage.vue'

// Stejné cesty jako Blazor: "/" (Home.razor) a "/{City}" (Map.razor).
// Mapová stránka se načítá líně – úvodní stránka tak nestahuje Leaflet.
export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'home', component: HomePage },
    { path: '/:city', name: 'map', component: () => import('./pages/MapPage.vue'), props: true },
  ],
})
