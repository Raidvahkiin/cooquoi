import { HttpResponse, http } from 'msw';
import { describe, expect, it } from 'vitest';
import { signIn } from '../../../../test/auth';
import { API_BASE_URL, meFixture } from '../../../../test/fixtures/household';
import { server } from '../../../../test/msw/node';
import { requestHeaders } from '../../../../test/request-headers';
import { getMe } from './get-me.query';

describe('getMe', () => {
  it('maps the profile and households of the signed-in user', async () => {
    await signIn();

    await expect(getMe()).resolves.toEqual({
      displayName: meFixture.profile.displayName,
      email: meFixture.profile.email,
      locale: meFixture.profile.locale,
      householdCount: 1,
    });
  });

  it('sends the user through login again when the profile does not exist', async () => {
    server.use(
      http.get(`${API_BASE_URL}/v1/me`, () =>
        HttpResponse.json(
          {
            status: 404,
            code: 'common.notFound',
            params: { resource: 'UserProfile' },
          },
          { status: 404 },
        ),
      ),
    );
    await signIn();
    requestHeaders.set('x-cooquoi-requested-path', '/pantry');

    await expect(getMe()).rejects.toMatchObject({
      digest: expect.stringContaining('/auth/login?returnTo=%2Fpantry'),
    });
  });
});
