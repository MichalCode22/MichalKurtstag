import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig({
  root: 'src',
  plugins: [react()],
  build: {
    outDir: '../dist',
    emptyOutDir: true
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5069',
        changeOrigin: true,
        secure: false
      }
    }
  },
  test: {
    environment: 'jsdom',
    include: ['**/*.test.{ts,tsx}']
  }
});