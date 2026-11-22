import { sealData } from 'iron-session';
import { HttpResponse, http } from 'msw';
import { NextRequest } from 'next/server';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { type AuthTokens, REQUESTED_PATH_HEADER } from '@/shared/auth';
import {
  identityEndpoints,
  tokenResponseFixture,
} from '../test/fixtures/identity';
import { server } from '../test/msw/node';
import { sessionStore } from '../test/session-store';
import { proxy } from './proxy';

const COOKIE = 'cooquoi_session';
const PASSWORD = 'test-session-password-at-least-32-chars';
const SID = 'session-id-1';

/** A browser request whose cookie points at a stored session with these tokens. */
async function requestWithSession(
  tokens: AuthTokens,
  ttlSeconds = 1800,
): Promise<NextRequest> {
  await sessionStore.set({ id: SID, data: { tokens }, ttlSeconds });
  const request = new NextRequest('http://localhost:3000/account');
  request.cookies.set(
    COOKIE,
    await sealData({ sid: SID }, { password: PASSWORD }),
  );
  return request;
}

/** iron-session appends a raw Set-Cookie header, so read that rather than `response.cookies`. */
function sessionCookieOf(response: Response): string | undefined {
  const header = response.headers
    .getSetCookie()
    .find((cookie) => cookie.startsWith(`${COOKIE}=`));
  return header?.slice(COOKIE.length + 1).split(';')[0];
}

const storedTokens = async () => (await sessionStore.get(SID))?.tokens;

const tokensExpiringIn = (ms: number): AuthTokens => ({
  accessToken: 'old-access',
  refreshToken: 'old-refresh',
  expiresAt: Date.now() + ms,
  refreshExpiresAt: Date.now() + 1_800_000,
});

describe('proxy', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('refreshes an expired access token before the page runs', async () => {
    let refreshRequest: URLSearchParams | undefined;
    server.use(
      http.post(identityEndpoints.token, async ({ request }) => {
        refreshRequest = new URLSearchParams(await request.text());
        return HttpResponse.json({
          ...tokenResponseFixture,
          access_token: 'new-access',
          refresh_token: 'new-refresh',
        });
      }),
    );

    await proxy(await requestWithSession(tokensExpiringIn(-1_000)));

    expect(refreshRequest?.get('grant_type')).toBe('refresh_token');
    expect(refreshRequest?.get('refresh_token')).toBe('old-refresh');
    expect(await storedTokens()).toMatchObject({
      accessToken: 'new-access',
      refreshToken: 'new-refresh',
    });
  });

  it('keeps the session id, so the page of the same request reads the new tokens', async () => {
    const response = await proxy(
      await requestWithSession(tokensExpiringIn(-1_000)),
    );

    expect(sessionCookieOf(response)).toBeUndefined();
    expect((await storedTokens())?.accessToken).toBe('access-token-1');
  });

  it('extends the session to the refresh token lifetime', async () => {
    vi.useFakeTimers({ toFake: ['Date'] });
    server.use(
      http.post(identityEndpoints.token, () =>
        HttpResponse.json({
          ...tokenResponseFixture,
          refresh_expires_in: 1800,
        }),
      ),
    );

    await proxy(await requestWithSession(tokensExpiringIn(-1_000), 10));
    vi.advanceTimersByTime(60_000);

    expect(await storedTokens()).toBeDefined();
  });

  it('refreshes a token that expires within the safety margin', async () => {
    await proxy(await requestWithSession(tokensExpiringIn(10_000)));

    expect((await storedTokens())?.accessToken).toBe('access-token-1');
  });

  it('leaves a valid session untouched', async () => {
    const response = await proxy(
      await requestWithSession(tokensExpiringIn(5 * 60_000)),
    );

    expect(sessionCookieOf(response)).toBeUndefined();
    expect((await storedTokens())?.accessToken).toBe('old-access');
  });

  it('tells the page which path was requested, for a login to come back to', async () => {
    const response = await proxy(
      new NextRequest('http://localhost:3000/account?tab=households'),
    );

    // Next.js applies `x-middleware-request-*` headers to the request the page reads.
    expect(
      response.headers.get(`x-middleware-request-${REQUESTED_PATH_HEADER}`),
    ).toBe('/account?tab=households');
  });

  it('signs the user out when Keycloak refuses the refresh', async () => {
    server.use(
      http.post(identityEndpoints.token, () =>
        HttpResponse.json({ error: 'invalid_grant' }, { status: 400 }),
      ),
    );

    const response = await proxy(
      await requestWithSession(tokensExpiringIn(-1_000)),
    );

    expect(sessionCookieOf(response)).toBe('');
    expect(await sessionStore.get(SID)).toBeUndefined();
  });
});
