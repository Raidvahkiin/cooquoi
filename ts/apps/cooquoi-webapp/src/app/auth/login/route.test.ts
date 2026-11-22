import { describe, expect, it } from 'vitest';
import { loadSession } from '@/shared/auth';
import { currentSession, signIn, startLoginAt } from '../../../../test/auth';
import { identityEndpoints } from '../../../../test/fixtures/identity';
import { sessionStore } from '../../../../test/session-store';

describe('GET /auth/login', () => {
  it('redirects to Keycloak with PKCE (S256), a state and the callback URL', async () => {
    const location = await startLoginAt('/auth/login');

    expect(`${location.origin}${location.pathname}`).toBe(
      identityEndpoints.authorization,
    );
    expect(Object.fromEntries(location.searchParams)).toMatchObject({
      client_id: 'cooquoi-webapp',
      response_type: 'code',
      redirect_uri: 'http://localhost:3000/auth/callback',
      scope: 'openid profile email',
      code_challenge_method: 'S256',
    });
    expect(location.searchParams.get('code_challenge')).toBeTruthy();
  });

  it('keeps the state and the page to return to in the session', async () => {
    const location = await startLoginAt('/auth/login?returnTo=/account');

    const { pendingLogin } = await currentSession();
    expect(pendingLogin?.state).toBe(location.searchParams.get('state'));
    expect(pendingLogin?.returnTo).toBe('/account');
  });

  it.each(['//evil.test/phish', 'https://evil.test', 'account'])(
    'returns to the home page instead of the unsafe target %s',
    async (returnTo) => {
      await startLoginAt(
        `/auth/login?returnTo=${encodeURIComponent(returnTo)}`,
      );

      expect((await currentSession()).pendingLogin?.returnTo).toBe('/');
    },
  );

  it('starts the login under a new session id, dropping the previous session', async () => {
    await signIn();
    const previousSid = (await loadSession())?.sid ?? '';

    await startLoginAt('/auth/login');

    expect((await loadSession())?.sid).not.toBe(previousSid);
    await expect(sessionStore.get(previousSid)).resolves.toBeUndefined();
  });
});
