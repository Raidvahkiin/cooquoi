import { defineProject } from 'vitest/config';

export default defineProject({
  root: __dirname,
  cacheDir: '../../node_modules/.vite/apps/cooquoi-api-e2e',
  resolve: {
    tsconfigPaths: true,
  },
  test: {
    name: '@cooquoi/api:e2e',
    globals: true,
    environment: 'node',
    include: ['test/**/*.e2e-spec.{js,mjs,cjs,ts,mts,cts}'],
  },
});
