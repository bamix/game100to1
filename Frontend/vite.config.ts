import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src')
    }
  },
  build: {
    outDir: '../Backend/Game100To1.Backend/wwwroot',
    emptyOutDir: true
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