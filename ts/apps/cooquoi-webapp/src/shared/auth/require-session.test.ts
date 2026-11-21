import { redirect } from 'next/navigation';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { requestHeaders } from '../../../test/request-headers';
import { getSession } from './session';
import { REQUESTED_PATH_HEADER, requireAccessToken } from './require-session';

vi.mock('next/navigation', () => ({
  redirect: vi.fn((url: string) => {
    throw new Error(`redirect ${url}`);
  }),
}));

describe('requireAccessToken', () => {
  beforeEach(() => {
    vi.mocked(redirect).mockClear();
  });

  it('returns the access token of the signed-in user', async () => {
    const session = await getSession();
    session.tokens = {
      accessToken: 'access',
      refreshToken: 'refresh',
      expiresAt: Date.now() + 60_000,
    };
    await session.save();

    await expect(requireAccessToken()).resolves.toBe('access');
  });

  it('sends a signed-out user to login, coming back to the requested page', async () => {
    requestHeaders.set(REQUESTED_PATH_HEADER, '/account?tab=households');

    await expect(requireAccessToken()).rejects.toThrow();

    expect(redirect).toHaveBeenCalledWith(
      '/auth/login?returnTo=%2Faccount%3Ftab%3Dhouseholds',
    );
  });

  it('comes back to the home page when the requested path is unknown', async () => {
    await expect(requireAccessToken()).rejects.toThrow();

    expect(redirect).toHaveBeenCalledWith('/auth/login?returnTo=%2F');
  });
});
