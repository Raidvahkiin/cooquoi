import { afterAll, afterEach, beforeAll, vi } from 'vitest';
import { cookieJar } from './cookie-jar';
import { server } from './msw/node';

vi.mock('next/headers', async () => {
  const { cookieJar: jar } = await import('./cookie-jar');
  return { cookies: async () => jar };
});

beforeAll(() => server.listen({ onUnhandledFrame: 'error' }));
afterEach(() => {
  server.resetHandlers();
  cookieJar.clear();
});
afterAll(() => server.close());
