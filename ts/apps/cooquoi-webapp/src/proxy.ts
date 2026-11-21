import { getIronSession } from 'iron-session';
import { type NextRequest, NextResponse } from 'next/server';
import {
  isExpiringSoon,
  REQUESTED_PATH_HEADER,
  refreshTokens,
  type SessionData,
  sessionOptions,
} from '@/shared/auth';

/**
 * Refreshes the access token before pages run: Server Components cannot write cookies,
 * the proxy can (ADR 0011). A failed refresh signs the user out. Also tells the page which
 * path was requested, so a login started from it can come back there.
 */
export async function proxy(request: NextRequest) {
  const headers = new Headers(request.headers);
  headers.set(
    REQUESTED_PATH_HEADER,
    `${request.nextUrl.pathname}${request.nextUrl.search}`,
  );
  const response = NextResponse.next({ request: { headers } });
  // Written through `response.cookies` (not a raw Set-Cookie header): Next.js then hands the
  // refreshed cookie to the page of this same request, not only to the browser.
  const session = await getIronSession<SessionData>(
    {
      get: (name) => request.cookies.get(name),
      set: response.cookies.set.bind(response.cookies),
    },
    sessionOptions(),
  );

  if (session.tokens && isExpiringSoon(session.tokens)) {
    try {
      session.tokens = await refreshTokens(session.tokens);
    } catch {
      session.tokens = undefined;
    }
    await session.save();
  }

  return response;
}

export const config = {
  // Pages only: skip auth routes, API routes, Next internals and static files.
  matcher: ['/((?!auth/|api/|_next/|favicon.ico|.*\\.[a-z0-9]+$).*)'],
};
