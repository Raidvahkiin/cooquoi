import { HttpResponse, http } from 'msw';
import { describe, expect, it } from 'vitest';
import { loadSession } from '@/shared/auth';
import { currentSession, signIn } from '../../../../test/auth';
import { identityEndpoints } from '../../../../test/fixtures/identity';
import { server } from '../../../../test/msw/node';
import { sessionStore } from '../../../../test/session-store';
import { GET as logout } from './route';

describe('GET /auth/logout', () => {
  it('ends the Keycloak session with the refresh token, without a confirmation page', async () => {
    let endSessionRequest: URLSearchParams | undefined;
    server.use(
      http.post(identityEndpoints.endSession, async ({ request }) => {
        endSessionRequest = new URLSearchParams(await request.text());
        return new HttpResponse(null, { status: 204 });
      }),
    );
    await signIn({ refreshToken: 'refresh-token-1' });

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
    expect((await currentSession()).tokens).toBeUndefined();
  });

  it('deletes the stored session', async () => {
    await signIn();
    const sid = (await loadSession())?.sid ?? '';

    await logout();

    await expect(sessionStore.get(sid)).resolves.toBeUndefined();
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
