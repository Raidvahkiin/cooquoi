import { defineProject } from 'vitest/config';

export default defineProject({
  root: __dirname,
  cacheDir: '../../node_modules/.vite/apps/cooquoi-api',
  resolve: {
    tsconfigPaths: true,
  },
  test: {
    name: '@cooquoi/api',
    globals: true,
    environment: 'node',
    include: ['src/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts}'],
  },
});
