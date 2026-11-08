import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import { router } from './router'
import { loadConfig } from './config'

// Bez top-level await – legacy build (SystemJS) ho ve starých prohlížečích nepodporuje.
loadConfig()
  .then(() => {
    createApp(App).use(createPinia()).use(router).mount('#app')
  })
  .catch((error) => {
    console.error('Nepodařilo se načíst konfiguraci aplikace', error)
    const app = document.getElementById('app')
    if (app) app.textContent = 'Aplikaci se nepodařilo spustit. / The application failed to start.'
  })
