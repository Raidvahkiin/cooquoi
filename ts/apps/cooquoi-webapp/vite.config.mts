import { playwright } from '@vitest/browser-playwright';
import { defineConfig } from 'vitest/config';

const testEnv = {
  APP_URL: 'http://localhost:3000',
  API_BASE_URL: 'http://api.test',
  COOQUOI_API_TIMEOUT_MS: '200',
  OIDC_ISSUER: 'https://identity.test/realms/cooquoi',
  OIDC_CLIENT_ID: 'cooquoi-webapp',
  OIDC_CLIENT_SECRET: 'test-client-secret',
  SESSION_PASSWORD: 'test-session-password-at-least-32-chars',
  REDIS_URL: 'redis://localhost:6379',
};

export default defineConfig({
  root: import.meta.dirname,
  cacheDir: '../../../node_modules/.vite/ts/apps/cooquoi-webapp',
  // tsconfig keeps `jsx: preserve` for Next.js; tests compile JSX themselves.
  oxc: { jsx: { runtime: 'automatic' } },
  resolve: {
    // Vitest does not read tsconfig `paths`: mirror `@/*` here.
    alias: {
      '@': `${import.meta.dirname}/src`,
      'server-only': `${import.meta.dirname}/test/server-only.ts`,
      'next/font/google': `${import.meta.dirname}/test/next-font-google.ts`,
    },
  },
  test: {
    projects: [
      {
        extends: true,
        test: {
          name: 'node',
          environment: 'node',
          include: ['src/**/*.test.ts'],
          exclude: ['src/**/*.integration.test.ts'],
          setupFiles: ['test/setup.ts'],
          env: testEnv,
        },
      },
      {
        extends: true,
        test: {
          // Against real services in Testcontainers (Docker must run); no MSW, which would intercept the Docker API.
          name: 'integration',
          environment: 'node',
          include: ['src/**/*.integration.test.ts'],
          env: testEnv,
          hookTimeout: 120_000,
        },
      },
      {
        extends: true,
        // The scan misses the JSX runtime injected by `oxc` (dev or prod, by NODE_ENV); found at run time it reloads the page mid-run.
        // Next.js client modules (`next/link`) read `process.env.__NEXT_*` at import.
        define: { 'process.env': '{}' },
        optimizeDeps: {
          include: ['react/jsx-dev-runtime', 'react/jsx-runtime'],
        },
        test: {
          // Components, in real Chromium (installed with `pnpm exec playwright install chromium`).
          name: 'browser',
          include: ['src/**/*.test.tsx'],
          browser: {
            enabled: true,
            provider: playwright(),
            headless: true,
            instances: [{ browser: 'chromium' }],
          },
        },
      },
    ],
  },
});
