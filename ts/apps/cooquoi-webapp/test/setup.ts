import { afterAll, afterEach, beforeAll, vi } from 'vitest';
import { cookieJar } from './cookie-jar';
import { server } from './msw/node';
import { clearRequestHeaders } from './request-headers';
import { clearSessionStore } from './session-store';

vi.mock('next/headers', async () => {
  const { cookieJar: jar } = await import('./cookie-jar');
  const { requestHeaders } = await import('./request-headers');
  return { cookies: async () => jar, headers: async () => requestHeaders };
});

vi.mock('@/shared/auth/session-store', async () => {
  const { sessionStore } = await import('./session-store');
  return { getSessionStore: () => sessionStore };
});

beforeAll(() => server.listen({ onUnhandledFrame: 'error' }));
afterEach(() => {
  server.resetHandlers();
  cookieJar.clear();
  clearRequestHeaders();
  clearSessionStore();
});
afterAll(() => server.close());
