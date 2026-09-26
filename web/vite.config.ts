import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// API ve SignalR isteklerini .NET backend'e (127.0.0.1:5099) yönlendir
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': { target: 'http://127.0.0.1:5099', changeOrigin: true },
      '/hubs': { target: 'http://127.0.0.1:5099', changeOrigin: true, ws: true },
    },
  },
});
