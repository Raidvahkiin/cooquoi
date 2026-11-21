import { sealData, unsealData } from 'iron-session';
import { HttpResponse, http } from 'msw';
import { NextRequest } from 'next/server';
import { describe, expect, it } from 'vitest';
import { REQUESTED_PATH_HEADER, type SessionData } from '@/shared/auth';
import {
  identityEndpoints,
  tokenResponseFixture,
} from '../test/fixtures/identity';
import { server } from '../test/msw/node';
import { proxy } from './proxy';

const COOKIE = 'cooquoi_session';
const PASSWORD = 'test-session-password-at-least-32-chars';

async function requestWithSession(session: SessionData): Promise<NextRequest> {
  const request = new NextRequest('http://localhost:3000/account');
  request.cookies.set(COOKIE, await sealData(session, { password: PASSWORD }));
  return request;
}

/** iron-session appends a raw Set-Cookie header, so read that rather than `response.cookies`. */
function sessionCookieOf(response: Response): string | undefined {
  const header = response.headers
    .getSetCookie()
    .find((cookie) => cookie.startsWith(`${COOKIE}=`));
  return header?.slice(COOKIE.length + 1).split(';')[0];
}

async function sessionWrittenBy(
  response: Response,
): Promise<SessionData | undefined> {
  const sealed = sessionCookieOf(response);
  return sealed
    ? unsealData<SessionData>(sealed, { password: PASSWORD })
    : undefined;
}

const tokensExpiringIn = (ms: number) => ({
  accessToken: 'old-access',
  refreshToken: 'old-refresh',
  expiresAt: Date.now() + ms,
});

describe('proxy', () => {
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

    const response = await proxy(
      await requestWithSession({ tokens: tokensExpiringIn(-1_000) }),
    );

    expect(refreshRequest?.get('grant_type')).toBe('refresh_token');
    expect(refreshRequest?.get('refresh_token')).toBe('old-refresh');
    expect((await sessionWrittenBy(response))?.tokens).toMatchObject({
      accessToken: 'new-access',
      refreshToken: 'new-refresh',
    });
  });

  it('hands the refreshed session to the page of the same request', async () => {
    const response = await proxy(
      await requestWithSession({ tokens: tokensExpiringIn(-1_000) }),
    );

    // Next.js merges this header into the request cookies that Server Components read.
    expect(response.headers.get('x-middleware-set-cookie')).toContain(
      `${COOKIE}=${sessionCookieOf(response)}`,
    );
  });

  it('refreshes a token that expires within the safety margin', async () => {
    const response = await proxy(
      await requestWithSession({ tokens: tokensExpiringIn(10_000) }),
    );

    expect((await sessionWrittenBy(response))?.tokens?.accessToken).toBe(
      'access-token-1',
    );
  });

  it('leaves a valid session untouched', async () => {
    const response = await proxy(
      await requestWithSession({ tokens: tokensExpiringIn(5 * 60_000) }),
    );

    expect(sessionCookieOf(response)).toBeUndefined();
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
      await requestWithSession({ tokens: tokensExpiringIn(-1_000) }),
    );

    expect(sessionCookieOf(response)).toBeDefined();
    expect((await sessionWrittenBy(response))?.tokens).toBeUndefined();
  });
});
