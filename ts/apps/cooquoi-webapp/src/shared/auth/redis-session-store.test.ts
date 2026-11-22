import { afterAll, describe, expect, it, vi } from 'vitest';
import { createRedisSessionStore } from './redis-session-store';

// Read on first use; nothing listens on port 1, so every connection attempt is refused.
vi.stubEnv('SESSION_STORE_URL', 'redis://127.0.0.1:1');

afterAll(() => {
  vi.unstubAllEnvs();
});

describe('redis session store without Redis', () => {
  it('fails fast instead of hanging', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    const startedAt = Date.now();

    await expect(createRedisSessionStore().get('any')).rejects.toThrow();

    expect(Date.now() - startedAt).toBeLessThan(3_000);
  }, 10_000);
});
