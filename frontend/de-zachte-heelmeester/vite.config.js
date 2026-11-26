import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  build: {
    // Build into the API's wwwroot, so frontend and API run on one address
    outDir: '../../backend/De-zachte-heelmeester/De-zachte-heelmeester/wwwroot',
    emptyOutDir: true,
  },
})