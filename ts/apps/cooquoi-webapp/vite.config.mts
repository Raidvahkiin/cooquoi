import { defineProject } from 'vitest/config';

export default defineProject({
  root: import.meta.dirname,
  cacheDir: '../../../node_modules/.vite/ts/apps/cooquoi-webapp',
  resolve: {
    // Vitest does not read tsconfig `paths`: mirror `@/*` here.
    alias: {
      '@': `${import.meta.dirname}/src`,
      'server-only': `${import.meta.dirname}/test/server-only.ts`,
    },
  },
  test: {
    name: 'webapp',
    environment: 'node',
    include: ['src/**/*.test.ts'],
    setupFiles: ['test/setup.ts'],
    env: {
      APP_URL: 'http://localhost:3000',
      API_BASE_URL: 'http://api.test',
      OIDC_ISSUER: 'http://identity.test/realms/cooquoi',
      OIDC_CLIENT_ID: 'cooquoi-webapp',
      OIDC_CLIENT_SECRET: 'test-client-secret',
      SESSION_PASSWORD: 'test-session-password-at-least-32-chars',
    },
  },
});
