import 'server-only';
import * as oidc from 'openid-client';
import { getEnv } from '@/shared/config/env';
import { getOidcConfig } from './oidc';
import { deleteSession, loadSession, rotateSession } from './session';
import { toAuthTokens, toSessionTtlSeconds } from './tokens';

const CALLBACK_PATH = '/auth/callback';
/** Time allowed to sign in at Keycloak and come back. */
const PENDING_LOGIN_TTL_S = 600;

/** Starts the Authorization Code + PKCE flow and returns Keycloak's login URL. */
export async function startLogin(returnTo: string): Promise<URL> {
  const config = await getOidcConfig();
  const codeVerifier = oidc.randomPKCECodeVerifier();
  const state = oidc.randomState();

  await rotateSession(
    { pendingLogin: { codeVerifier, state, returnTo: toSafePath(returnTo) } },
    PENDING_LOGIN_TTL_S,
  );

  return oidc.buildAuthorizationUrl(config, {
    redirect_uri: new URL(CALLBACK_PATH, getEnv().APP_URL).href,
    scope: 'openid profile email',
    code_challenge: await oidc.calculatePKCECodeChallenge(codeVerifier),
    code_challenge_method: 'S256',
    state,
  });
}

/** Exchanges the authorization code, stores the tokens, returns where to go next. */
export async function completeLogin(callbackSearch: string): Promise<string> {
  const config = await getOidcConfig();
  const pending = (await loadSession())?.data.pendingLogin;
  if (!pending) {
    throw new Error('No login in progress');
  }

  const callbackUrl = new URL(
    `${CALLBACK_PATH}${callbackSearch}`,
    getEnv().APP_URL,
  );
  const response = await oidc.authorizationCodeGrant(config, callbackUrl, {
    pkceCodeVerifier: pending.codeVerifier,
    expectedState: pending.state,
  });

  const tokens = toAuthTokens(response);
  await rotateSession({ tokens }, toSessionTtlSeconds(tokens));
  return pending.returnTo;
}

/**
 * Ends the Keycloak SSO session server-side (no confirmation page, since the ID token
 * is not kept), then the local session. Returns where to send the user.
 */
export async function logout(): Promise<URL> {
  const refreshToken = (await loadSession())?.data.tokens?.refreshToken;
  await deleteSession();

  if (refreshToken) {
    await endKeycloakSession(refreshToken);
  }
  return new URL('/', getEnv().APP_URL);
}

async function endKeycloakSession(refreshToken: string): Promise<void> {
  const config = await getOidcConfig();
  const endpoint = config.serverMetadata().end_session_endpoint;
  if (!endpoint) {
    return;
  }
  const env = getEnv();
  await fetch(endpoint, {
    method: 'POST',
    headers: { 'content-type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({
      client_id: env.OIDC_CLIENT_ID,
      client_secret: env.OIDC_CLIENT_SECRET,
      refresh_token: refreshToken,
    }),
  });
}

/** Only same-site paths: never redirect to another host after login. */
function toSafePath(path: string): string {
  return path.startsWith('/') && !path.startsWith('//') ? path : '/';
}
