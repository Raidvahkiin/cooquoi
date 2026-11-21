import { HttpResponse, http } from 'msw';
import { describe, expect, it } from 'vitest';
import { getSession } from '@/shared/auth';
import { identityEndpoints } from '../../../../test/fixtures/identity';
import { server } from '../../../../test/msw/node';
import { GET as logout } from './route';

async function signIn() {
  const session = await getSession();
  session.tokens = {
    accessToken: 'access-token-1',
    refreshToken: 'refresh-token-1',
    expiresAt: Date.now() + 60_000,
  };
  await session.save();
}

describe('GET /auth/logout', () => {
  it('ends the Keycloak session with the refresh token, without a confirmation page', async () => {
    let endSessionRequest: URLSearchParams | undefined;
    server.use(
      http.post(identityEndpoints.endSession, async ({ request }) => {
        endSessionRequest = new URLSearchParams(await request.text());
        return new HttpResponse(null, { status: 204 });
      }),
    );
    await signIn();

    await logout();

    expect(Object.fromEntries(endSessionRequest ?? [])).toEqual({
      client_id: 'cooquoi-webapp',
      client_secret: 'test-client-secret',
      refresh_token: 'refresh-token-1',
    });
  });

  it('clears the local session and redirects home', async () => {
    await signIn();

    const response = await logout();

    expect(response.headers.get('location')).toBe('http://localhost:3000/');
    expect((await getSession()).tokens).toBeUndefined();
  });

  it('does not call Keycloak when nobody is signed in', async () => {
    let isEndSessionCalled = false;
    server.use(
      http.post(identityEndpoints.endSession, () => {
        isEndSessionCalled = true;
        return new HttpResponse(null, { status: 204 });
      }),
    );

    await logout();

    expect(isEndSessionCalled).toBe(false);
  });
});
