import 'server-only';
import * as oidc from 'openid-client';
import { getOidcConfig } from './oidc';
import type { AuthTokens } from './session';

/** Refresh slightly before expiry, so a request never carries an expired token. */
const EXPIRY_MARGIN_MS = 30_000;
/** Keycloak's default SSO idle timeout, when the response gives no refresh token lifetime. */
const DEFAULT_REFRESH_LIFETIME_S = 1800;

export function toAuthTokens(response: oidc.TokenEndpointResponse): AuthTokens {
  if (!response.refresh_token) {
    throw new Error('Keycloak returned no refresh token');
  }
  const refreshLifetime =
    Number(response.refresh_expires_in) || DEFAULT_REFRESH_LIFETIME_S;
  return {
    accessToken: response.access_token,
    refreshToken: response.refresh_token,
    expiresAt: Date.now() + (response.expires_in ?? 0) * 1000,
    refreshExpiresAt: Date.now() + refreshLifetime * 1000,
  };
}

/** TTL of a signed-in session: the refresh token's remaining lifetime (ADR 0019 §2). */
export function toSessionTtlSeconds(tokens: AuthTokens): number {
  return Math.max(1, Math.ceil((tokens.refreshExpiresAt - Date.now()) / 1000));
}

export function isExpiringSoon(tokens: AuthTokens): boolean {
  return tokens.expiresAt - EXPIRY_MARGIN_MS <= Date.now();
}

/** In-flight refreshes by refresh token: Keycloak accepts each one once, so parallel requests share the call. */
const refreshing = new Map<string, Promise<AuthTokens>>();

export function refreshTokens(tokens: AuthTokens): Promise<AuthTokens> {
  const { refreshToken } = tokens;
  let refresh = refreshing.get(refreshToken);
  if (!refresh) {
    refresh = getOidcConfig()
      .then((config) => oidc.refreshTokenGrant(config, refreshToken))
      .then(toAuthTokens)
      .finally(() => refreshing.delete(refreshToken));
    refreshing.set(refreshToken, refresh);
  }
  return refresh;
}
