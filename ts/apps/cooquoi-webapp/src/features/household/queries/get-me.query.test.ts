import { describe, expect, it } from 'vitest';
import { getSession } from '@/shared/auth';
import { meFixture } from '../../../../test/fixtures/household';
import { getMe } from './get-me.query';

describe('getMe', () => {
  it('maps the profile and households of the signed-in user', async () => {
    const session = await getSession();
    session.tokens = {
      accessToken: 'access',
      refreshToken: 'refresh',
      expiresAt: Date.now() + 60_000,
    };
    await session.save();

    await expect(getMe()).resolves.toEqual({
      displayName: meFixture.profile.displayName,
      email: meFixture.profile.email,
      locale: meFixture.profile.locale,
      householdCount: 1,
    });
  });
});
