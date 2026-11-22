import { redirect } from 'next/navigation';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { signIn } from '../../../test/auth';
import { requestHeaders } from '../../../test/request-headers';
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
  afterEach(() => {
    vi.useRealTimers();
  });

  it('returns the access token of the signed-in user', async () => {
    await signIn({ accessToken: 'access' });

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

  it('sends the user to login when the stored session has expired', async () => {
    vi.useFakeTimers({ toFake: ['Date'] });
    await signIn();
    // signIn stores the session for 1800 s.
    vi.advanceTimersByTime(1_801_000);

    await expect(requireAccessToken()).rejects.toThrow();

    expect(redirect).toHaveBeenCalledWith('/auth/login?returnTo=%2F');
  });
});
