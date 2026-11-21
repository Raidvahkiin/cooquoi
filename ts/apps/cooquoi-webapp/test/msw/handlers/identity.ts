import { HttpResponse, http } from 'msw';
import {
  discoveryFixture,
  ISSUER,
  identityEndpoints,
  tokenResponseFixture,
} from '../../fixtures/identity';

/** Keycloak, as seen by openid-client: discovery, token endpoint, end session. */
export const identityHandlers = [
  http.get(`${ISSUER}/.well-known/openid-configuration`, () =>
    HttpResponse.json(discoveryFixture),
  ),
  http.post(identityEndpoints.token, () =>
    HttpResponse.json(tokenResponseFixture),
  ),
  http.post(
    identityEndpoints.endSession,
    () => new HttpResponse(null, { status: 204 }),
  ),
];
