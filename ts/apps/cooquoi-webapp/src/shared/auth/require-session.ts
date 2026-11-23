import 'server-only';
import { headers } from 'next/headers';
import { redirect } from 'next/navigation';
import { loadSession } from './session';

/** Set by `proxy.ts` on every page request; never trusted from the browser, the proxy overwrites it. */
export const REQUESTED_PATH_HEADER = 'x-cooquoi-requested-path';

/**
 * Access token of the signed-in user; redirects to login when there is none or its session
 * expired, coming back to the requested page afterwards. Tokens are refreshed beforehand by
 * `proxy.ts`, which runs before every page.
 */
export async function requireAccessToken(): Promise<string> {
  const tokens = (await loadSession())?.data.tokens;
  if (!tokens) {
    return requireLogin();
  }
  return tokens.accessToken;
}

/** Sends the user through login, coming back to the requested page afterwards. */
export async function requireLogin(): Promise<never> {
  const returnTo = (await headers()).get(REQUESTED_PATH_HEADER) ?? '/';
  redirect(`/auth/login?returnTo=${encodeURIComponent(returnTo)}`);
}
