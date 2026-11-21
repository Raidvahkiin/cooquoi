export const ISSUER = 'http://identity.test/realms/cooquoi';

export const identityEndpoints = {
  authorization: `${ISSUER}/protocol/openid-connect/auth`,
  token: `${ISSUER}/protocol/openid-connect/token`,
  endSession: `${ISSUER}/protocol/openid-connect/logout`,
};

/** Discovery document, trimmed to what openid-client reads. */
export const discoveryFixture = {
  issuer: ISSUER,
  authorization_endpoint: identityEndpoints.authorization,
  token_endpoint: identityEndpoints.token,
  end_session_endpoint: identityEndpoints.endSession,
  jwks_uri: `${ISSUER}/protocol/openid-connect/certs`,
};

export const tokenResponseFixture = {
  access_token: 'access-token-1',
  refresh_token: 'refresh-token-1',
  token_type: 'Bearer',
  expires_in: 300,
};
