import 'server-only';
import * as oidc from 'openid-client';
import { getOidcConfig } from './oidc';
import type { AuthTokens } from './session';

/** Refresh slightly before expiry, so a request never carries an expired token. */
const EXPIRY_MARGIN_MS = 30_000;

export function toAuthTokens(response: oidc.TokenEndpointResponse): AuthTokens {
  if (!response.refresh_token) {
    throw new Error('Keycloak returned no refresh token');
  }
  return {
    accessToken: response.access_token,
    refreshToken: response.refresh_token,
    expiresAt: Date.now() + (response.expires_in ?? 0) * 1000,
  };
}

export function isExpiringSoon(tokens: AuthTokens): boolean {
  return tokens.expiresAt - EXPIRY_MARGIN_MS <= Date.now();
}

export async function refreshTokens(tokens: AuthTokens): Promise<AuthTokens> {
  const config = await getOidcConfig();
  const response = await oidc.refreshTokenGrant(config, tokens.refreshToken);
  return toAuthTokens(response);
}
