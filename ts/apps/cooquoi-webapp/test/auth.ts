import { NextRequest } from 'next/server';
import { GET as startLogin } from '@/app/auth/login/route';
import { type AuthTokens, loadSession, saveSession } from '@/shared/auth';
import type { SessionData } from '@/shared/auth/session';

const APP_URL = 'http://localhost:3000';

export function appRequest(path: string): NextRequest {
  return new NextRequest(new URL(path, APP_URL));
}

/** Runs the login route like a browser would and returns the Keycloak redirect. */
export async function startLoginAt(path: string): Promise<URL> {
  const response = await startLogin(appRequest(path));
  return new URL(response.headers.get('location') ?? '');
}

/** Signs a user in for the current test, with tokens valid for a minute. */
export async function signIn(tokens: Partial<AuthTokens> = {}): Promise<void> {
  await saveSession({
    data: {
      tokens: {
        accessToken: 'access',
        refreshToken: 'refresh',
        expiresAt: Date.now() + 60_000,
        refreshExpiresAt: Date.now() + 1_800_000,
        ...tokens,
      },
    },
    ttlSeconds: 1800,
  });
}

/** What the store holds for the session cookie of the current test. */
export async function currentSession(): Promise<SessionData> {
  return (await loadSession())?.data ?? {};
}
