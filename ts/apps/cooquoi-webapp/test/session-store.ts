import { createMemorySessionStore } from '@/shared/auth/memory-session-store';
import type { SessionStore } from '@/shared/auth/session-store';

let store = createMemorySessionStore();

/** Stands in for Redis: `test/setup.ts` mocks `getSessionStore()` to return it. */
export const sessionStore: SessionStore = {
  get: (id) => store.get(id),
  set: (session) => store.set(session),
  delete: (id) => store.delete(id),
};

export function clearSessionStore(): void {
  store = createMemorySessionStore();
}
