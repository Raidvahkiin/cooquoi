import { HttpResponse, http } from 'msw';
import { describe, expect, it } from 'vitest';
import { loadSession } from '@/shared/auth';
import {
  appRequest,
  currentSession,
  startLoginAt,
} from '../../../../test/auth';
import { cookieJar } from '../../../../test/cookie-jar';
import { API_BASE_URL } from '../../../../test/fixtures/household';
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

    const { pendingLogin } = await currentSession();
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
    const { tokens } = await currentSession();
    expect(tokens).toMatchObject({
      accessToken: 'access-token-1',
      refreshToken: 'refresh-token-1',
    });
    expect(tokens?.expiresAt).toBeGreaterThan(Date.now());
  });

  it('signs in at the API once with the new access token, before redirecting', async () => {
    const calls: Request[] = [];
    server.use(
      http.post(`${API_BASE_URL}/v1/me\\:sign-in`, ({ request }) => {
        calls.push(request);
        return new HttpResponse(null, { status: 204 });
      }),
    );

    const response = await callbackFor('/auth/login?returnTo=/account');

    expect(calls).toHaveLength(1);
    expect(calls[0]?.headers.get('authorization')).toBe(
      'Bearer access-token-1',
    );
    expect(calls[0]?.headers.get('idempotency-key')).toMatch(
      /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/,
    );
    expect(response.headers.get('location')).toBe(
      'http://localhost:3000/account',
    );
  });

  it('fails instead of redirecting when the API sign-in fails', async () => {
    server.use(
      http.post(
        `${API_BASE_URL}/v1/me\\:sign-in`,
        () => new HttpResponse(null, { status: 500 }),
      ),
    );

    await expect(callbackFor('/auth/login')).rejects.toThrow();
  });

  it('moves the signed-in session to a new id', async () => {
    await startLoginAt('/auth/login');
    const pendingSid = (await loadSession())?.sid;

    await callbackFor('/auth/login');

    const sid = (await loadSession())?.sid;
    expect(sid).toBeDefined();
    expect(sid).not.toBe(pendingSid);
  });

  it('keeps only a small session id in the cookie', async () => {
    await callbackFor('/auth/login');

    // The tokens alone would take several KB (ADR 0011); the sealed id stays a few hundred bytes.
    expect(cookieJar.get('cooquoi_session')?.value.length).toBeLessThan(400);
  });

  it('rejects a callback whose state does not match the login in progress', async () => {
    await startLoginAt('/auth/login');

    await expect(
      completeLogin(appRequest('/auth/callback?code=c&state=forged')),
    ).rejects.toThrow();
    expect((await currentSession()).tokens).toBeUndefined();
  });

  it('rejects a callback without a login in progress', async () => {
    await expect(
      completeLogin(appRequest('/auth/callback?code=c&state=s')),
    ).rejects.toThrow('No login in progress');
  });
});
