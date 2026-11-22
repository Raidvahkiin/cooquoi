import 'server-only';
import { getIronSession, type SessionOptions } from 'iron-session';
import { cookies } from 'next/headers';
import { getEnv } from '@/shared/config/env';
import { createSessionId } from './session-id';
import { getSessionStore } from './session-store';

export type AuthTokens = {
  accessToken: string;
  refreshToken: string;
  /** Epoch milliseconds. */
  expiresAt: number;
  /** Epoch milliseconds: the session lives as long as the refresh token (ADR 0019 §2). */
  refreshExpiresAt: number;
};

/** Kept between the redirect to Keycloak and the callback. */
type PendingLogin = {
  codeVerifier: string;
  state: string;
  returnTo: string;
};

/** Stored server-side under the session id (ADR 0019 §1); the browser never sees it. */
export type SessionData = {
  tokens?: AuthTokens;
  pendingLogin?: PendingLogin;
};

/** The cookie carries only the session id. */
type SessionCookie = { sid?: string };

/**
 * Where the session cookie is read and written: `cookies()` of `next/headers` by default,
 * the request and response cookies in `proxy.ts`. Same shape as iron-session's cookie store.
 */
export type CookieAccess = {
  get: (name: string) => { name: string; value: string } | undefined;
  set: {
    (name: string, value: string, options?: Record<string, unknown>): void;
    (options: { name: string; value: string }): void;
  };
};

type LoadedSession = { sid: string; data: SessionData };

type SessionWrite = {
  data: SessionData;
  /** Restarts the session's lifetime. */
  ttlSeconds: number;
  access?: CookieAccess;
};

function sessionOptions(): SessionOptions {
  return {
    password: getEnv().SESSION_PASSWORD,
    cookieName: 'cooquoi_session',
    cookieOptions: {
      httpOnly: true,
      secure: true,
      sameSite: 'lax',
      path: '/',
    },
  };
}

async function openCookie(access?: CookieAccess) {
  return getIronSession<SessionCookie>(
    access ?? (await cookies()),
    sessionOptions(),
  );
}

/** Session of the current request; nothing when there is no cookie or its key is gone (expired, Redis restarted). */
export async function loadSession(
  access?: CookieAccess,
): Promise<LoadedSession | undefined> {
  const { sid } = await openCookie(access);
  if (!sid) return undefined;
  const data = await getSessionStore().get(sid);
  return data && { sid, data };
}

/** Writes the session with this TTL (restarting it), creating the id and cookie when missing. */
export async function saveSession({
  data,
  ttlSeconds,
  access,
}: SessionWrite): Promise<void> {
  const cookie = await openCookie(access);
  if (!cookie.sid) {
    cookie.sid = createSessionId();
    await cookie.save();
  }
  await getSessionStore().set({ id: cookie.sid, data, ttlSeconds });
}

/** Writes the session under a new id, dropping the old one: an id known before a login is worthless after it (no fixation). */
export async function rotateSession(
  data: SessionData,
  ttlSeconds: number,
): Promise<void> {
  const cookie = await openCookie();
  if (cookie.sid) await getSessionStore().delete(cookie.sid);
  cookie.sid = createSessionId();
  await getSessionStore().set({ id: cookie.sid, data, ttlSeconds });
  await cookie.save();
}

/** Deletes the stored session and the cookie. */
export async function deleteSession(access?: CookieAccess): Promise<void> {
  const cookie = await openCookie(access);
  if (cookie.sid) await getSessionStore().delete(cookie.sid);
  cookie.destroy();
}
