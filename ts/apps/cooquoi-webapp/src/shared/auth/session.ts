import 'server-only';
import { getIronSession, type SessionOptions } from 'iron-session';
import { cookies } from 'next/headers';
import { getEnv } from '@/shared/config/env';

export type AuthTokens = {
  accessToken: string;
  refreshToken: string;
  /** Epoch milliseconds. */
  expiresAt: number;
};

/** Kept between the redirect to Keycloak and the callback. */
export type PendingLogin = {
  codeVerifier: string;
  state: string;
  returnTo: string;
};

/** Only what the API calls need: the cookie must stay under ~4 KB (ADR 0011). */
export type SessionData = {
  tokens?: AuthTokens;
  pendingLogin?: PendingLogin;
};

export function sessionOptions(): SessionOptions {
  return {
    password: getEnv().SESSION_PASSWORD,
    cookieName: 'cooquoi_session',
    cookieOptions: {
      httpOnly: true,
      secure: process.env.NODE_ENV === 'production',
      sameSite: 'lax',
      path: '/',
    },
  };
}

/** Session of the current request, for Server Components, Server Actions and Route Handlers. */
export async function getSession() {
  return getIronSession<SessionData>(await cookies(), sessionOptions());
}
