import { HttpResponse, http } from 'msw';
import { describe, expect, it } from 'vitest';
import { getSession } from '@/shared/auth';
import { appRequest, startLoginAt } from '../../../../test/auth';
import {
  identityEndpoints,
  tokenResponseFixture,
} from '../../../../test/fixtures/identity';
import { server } from '../../../../test/msw/node';
import { GET as completeLogin } from './route';

async function callbackFor(loginPath: string, code = 'auth-code-1') {
  const state = (await startLoginAt(loginPath)).searchParams.get('state');
  return completeLogin(
    appRequest(`/auth/callback?code=${code}&state=${state}`),
  );
}

describe('GET /auth/callback', () => {
  it('exchanges the code with the PKCE verifier kept at login', async () => {
    let tokenRequest: URLSearchParams | undefined;
    server.use(
      http.post(identityEndpoints.token, async ({ request }) => {
        tokenRequest = new URLSearchParams(await request.text());
        return HttpResponse.json(tokenResponseFixture);
      }),
    );

    await callbackFor('/auth/login');

    const { pendingLogin } = await getSession();
    expect(pendingLogin).toBeUndefined();
    expect(Object.fromEntries(tokenRequest ?? [])).toMatchObject({
      grant_type: 'authorization_code',
      code: 'auth-code-1',
      redirect_uri: 'http://localhost:3000/auth/callback',
    });
    expect(tokenRequest?.get('code_verifier')).toBeTruthy();
  });

  it('stores the tokens and redirects to the page the user started from', async () => {
    const response = await callbackFor('/auth/login?returnTo=/account');

    expect(response.headers.get('location')).toBe(
      'http://localhost:3000/account',
    );
    const { tokens } = await getSession();
    expect(tokens).toMatchObject({
      accessToken: 'access-token-1',
      refreshToken: 'refresh-token-1',
    });
    expect(tokens?.expiresAt).toBeGreaterThan(Date.now());
  });

  it('rejects a callback whose state does not match the login in progress', async () => {
    await startLoginAt('/auth/login');

    await expect(
      completeLogin(appRequest('/auth/callback?code=c&state=forged')),
    ).rejects.toThrow();
    expect((await getSession()).tokens).toBeUndefined();
  });

  it('rejects a callback without a login in progress', async () => {
    await expect(
      completeLogin(appRequest('/auth/callback?code=c&state=s')),
    ).rejects.toThrow('No login in progress');
  });
});
