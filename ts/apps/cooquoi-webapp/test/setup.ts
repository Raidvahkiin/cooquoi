import { afterAll, afterEach, beforeAll, vi } from 'vitest';
import { cookieJar } from './cookie-jar';
import { clearRequestHeaders } from './request-headers';
import { server } from './msw/node';

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
