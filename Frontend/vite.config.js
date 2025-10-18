import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  build: {
    outDir: '../Backend/Game100To1.Backend/wwwroot',
    emptyOutDir: true,
    rollupOptions: {
      input: {
        main: './src/main.js'
      }
    }
  },
  server: {
    proxy: {
      '/api': 'http://localhost:5000',
      '/gamehub': {
        target: 'ws://localhost:5000',
        ws: true
      }
    }
  }
})