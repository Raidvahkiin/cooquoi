import type { SessionData } from './session';
import type { SessionStore } from './session-store';

type Entry = { data: SessionData; expiresAt: number };

/** In-process store with the same TTL semantics as Redis, for tests. */
export function createMemorySessionStore(): SessionStore {
  const entries = new Map<string, Entry>();

  function readEntry(id: string): Entry | undefined {
    const entry = entries.get(id);
    if (entry && Date.now() >= entry.expiresAt) {
      entries.delete(id);
      return undefined;
    }
    return entry;
  }

  return {
    async get(id) {
      const entry = readEntry(id);
      // A copy, as a real store deserializes: callers never share the stored object.
      return entry && structuredClone(entry.data);
    },
    async set({ id, data, ttlSeconds }) {
      entries.set(id, {
        data: structuredClone(data),
        expiresAt: Date.now() + ttlSeconds * 1000,
      });
    },
    async extend(id, ttlSeconds) {
      const entry = readEntry(id);
      if (entry) entry.expiresAt = Date.now() + ttlSeconds * 1000;
    },
    async delete(id) {
      entries.delete(id);
    },
  };
}
