import { afterAll, afterEach, beforeAll, vi } from 'vitest';
import { cookieJar } from './cookie-jar';
import { server } from './msw/node';
import { clearRequestHeaders } from './request-headers';

vi.mock('next/headers', async () => {
  const { cookieJar: jar } = await import('./cookie-jar');
  const { requestHeaders } = await import('./request-headers');
  return { cookies: async () => jar, headers: async () => requestHeaders };
});

beforeAll(() => server.listen({ onUnhandledFrame: 'error' }));
afterEach(() => {
  server.resetHandlers();
  cookieJar.clear();
  clearRequestHeaders();
});
afterAll(() => server.close());
