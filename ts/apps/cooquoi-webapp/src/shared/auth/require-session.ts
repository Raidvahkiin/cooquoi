import 'server-only';
import { headers } from 'next/headers';
import { redirect } from 'next/navigation';
import { getSession } from './session';

/** Set by `proxy.ts` on every page request; never trusted from the browser, the proxy overwrites it. */
export const REQUESTED_PATH_HEADER = 'x-cooquoi-requested-path';

/**
 * Access token of the signed-in user; redirects to login when there is none, coming back to
 * the requested page afterwards. Tokens are refreshed beforehand by `proxy.ts`, since pages
 * cannot write cookies.
 */
export async function requireAccessToken(): Promise<string> {
  const session = await getSession();
  if (!session.tokens) {
    const returnTo = (await headers()).get(REQUESTED_PATH_HEADER) ?? '/';
    redirect(`/auth/login?returnTo=${encodeURIComponent(returnTo)}`);
  }
  return session.tokens.accessToken;
}
