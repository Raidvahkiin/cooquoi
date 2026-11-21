import { getIronSession } from 'iron-session';
import { type NextRequest, NextResponse } from 'next/server';
import { isExpiringSoon, refreshTokens, sessionOptions, type SessionData } from '@/shared/auth';

/**
 * Refreshes the access token before pages run: Server Components cannot write cookies,
 * the proxy can (ADR 0011). A failed refresh signs the user out.
 */
export async function proxy(request: NextRequest) {
  const response = NextResponse.next();
  const session = await getIronSession<SessionData>(request, response, sessionOptions());

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
