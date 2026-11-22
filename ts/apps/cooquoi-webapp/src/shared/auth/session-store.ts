import 'server-only';
import { createRedisSessionStore } from './redis-session-store';
import type { SessionData } from './session';

type StoredSession = {
  id: string;
  data: SessionData;
  /** The session is gone after this many seconds unless extended. */
  ttlSeconds: number;
};

/** Server-side session storage keyed by session id; the cookie carries only the id (ADR 0019). */
export type SessionStore = {
  /** `undefined` when the session is missing, expired or unreadable. */
  get(id: string): Promise<SessionData | undefined>;
  set(session: StoredSession): Promise<void>;
  /** Restarts the TTL of an existing session; a missing one stays missing. */
  extend(id: string, ttlSeconds: number): Promise<void>;
  delete(id: string): Promise<void>;
};

let cachedStore: SessionStore | undefined;

export function getSessionStore(): SessionStore {
  cachedStore ??= createRedisSessionStore();
  return cachedStore;
}
