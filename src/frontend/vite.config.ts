import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: { '@': path.resolve(__dirname, './src') }
  },
  server: {
    port: 3000,
    proxy: {
      '/api/users': 'http://localhost:5001',
      '/api/listings': 'http://localhost:5002',
      '/api/bookings': 'http://localhost:5003',
      '/api/payments': 'http://localhost:5004',
      '/api/reviews': 'http://localhost:5006',
    }
  }
})
