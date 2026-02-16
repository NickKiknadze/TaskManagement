import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5038',
        changeOrigin: true,
        secure: false,
      },
      '/hubs/realtime': {
        target: 'http://localhost:5038',
        changeOrigin: true,
        ws: true,
      }
    }
  }
})
