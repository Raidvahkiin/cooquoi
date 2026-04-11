import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import tsconfigPaths from 'vite-tsconfig-paths';
import { defineProject } from 'vitest/config';

export default defineProject(() => ({
  root: __dirname,
  cacheDir: '../../../node_modules/.vite/libs/utils-react',
  plugins: [react(), tailwindcss(), tsconfigPaths()],
  test: {
    name: '@libs/utils-react',
    watch: false,
    globals: true,
    environment: 'jsdom',
    passWithNoTests: true,
    include: ['{src,tests}/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}'],
  },
}));
