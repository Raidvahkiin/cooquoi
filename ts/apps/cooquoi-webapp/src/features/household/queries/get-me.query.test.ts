import { describe, expect, it } from 'vitest';
import { signIn } from '../../../../test/auth';
import { meFixture } from '../../../../test/fixtures/household';
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
});
