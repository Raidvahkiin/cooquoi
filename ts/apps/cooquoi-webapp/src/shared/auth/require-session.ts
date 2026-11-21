import 'server-only';
import { redirect } from 'next/navigation';
import { getSession } from './session';

/**
 * Access token of the signed-in user; redirects to login when there is none.
 * Tokens are refreshed beforehand by `proxy.ts`, since pages cannot write cookies.
 */
export async function requireAccessToken(): Promise<string> {
  const session = await getSession();
  if (!session.tokens) {
    redirect('/auth/login');
  }
  return session.tokens.accessToken;
}
