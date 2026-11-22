import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { createMemorySessionStore } from './memory-session-store';
import type { SessionData } from './session';

const data: SessionData = {
  tokens: {
    accessToken: 'access',
    refreshToken: 'refresh',
    expiresAt: 1_000,
    refreshExpiresAt: 2_000,
  },
};

describe('memory session store', () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  it('returns the stored session until its TTL runs out', async () => {
    const store = createMemorySessionStore();
    await store.set({ id: 'a', data, ttlSeconds: 60 });

    vi.advanceTimersByTime(59_999);
    expect(await store.get('a')).toEqual(data);

    vi.advanceTimersByTime(1);
    expect(await store.get('a')).toBeUndefined();
  });

  it('restarts the TTL when the session is written again', async () => {
    const store = createMemorySessionStore();
    await store.set({ id: 'a', data, ttlSeconds: 60 });

    vi.advanceTimersByTime(50_000);
    await store.set({ id: 'a', data, ttlSeconds: 60 });
    vi.advanceTimersByTime(50_000);

    expect(await store.get('a')).toEqual(data);
  });

  it('forgets a deleted session', async () => {
    const store = createMemorySessionStore();
    await store.set({ id: 'a', data, ttlSeconds: 60 });

    await store.delete('a');

    expect(await store.get('a')).toBeUndefined();
  });

  it('returns undefined for an unknown id', async () => {
    expect(await createMemorySessionStore().get('missing')).toBeUndefined();
  });

  it('keeps the stored value apart from the caller objects', async () => {
    const store = createMemorySessionStore();
    const written = structuredClone(data);
    await store.set({ id: 'a', data: written, ttlSeconds: 60 });

    written.tokens = undefined;

    expect(await store.get('a')).toEqual(data);
  });
});
