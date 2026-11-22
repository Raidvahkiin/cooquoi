import { type NextRequest, NextResponse } from 'next/server';
import {
  type AuthTokens,
  type CookieAccess,
  deleteSession,
  isExpiringSoon,
  loadSession,
  REQUESTED_PATH_HEADER,
  refreshTokens,
  saveSession,
  toSessionTtlSeconds,
} from '@/shared/auth';

/**
 * Refreshes the access token before pages run, extending the session's TTL (ADR 0011, 0019).
 * A failed refresh signs the user out. Also tells the page which
 * path was requested, so a login started from it can come back there.
 */
export async function proxy(request: NextRequest) {
  const headers = new Headers(request.headers);
  headers.set(
    REQUESTED_PATH_HEADER,
    `${request.nextUrl.pathname}${request.nextUrl.search}`,
  );
  const response = NextResponse.next({ request: { headers } });
  // Written through `response.cookies` (not a raw Set-Cookie header): Next.js then hands a
  // cleared cookie to the page of this same request, not only to the browser.
  const access: CookieAccess = {
    get: (name) => request.cookies.get(name),
    set: response.cookies.set.bind(response.cookies),
  };
  const tokens = (await loadSession(access))?.data.tokens;

  if (tokens && isExpiringSoon(tokens)) {
    await refreshOrSignOut(tokens, access);
  }

  return response;
}

async function refreshOrSignOut(
  tokens: AuthTokens,
  access: CookieAccess,
): Promise<void> {
  let refreshed: AuthTokens;
  try {
    refreshed = await refreshTokens(tokens);
  } catch {
    await deleteSession(access);
    return;
  }
  // Outside the try: a store error goes to the error page, it does not sign the user out.
  await saveSession({
    data: { tokens: refreshed },
    ttlSeconds: toSessionTtlSeconds(refreshed),
    access,
  });
}

export const config = {
  // Pages only: skip auth routes, API routes, Next internals and static files.
  matcher: ['/((?!auth/|api/|_next/|favicon.ico|.*\\.[a-z0-9]+$).*)'],
};
