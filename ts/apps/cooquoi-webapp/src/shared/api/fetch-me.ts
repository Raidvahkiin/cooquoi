import 'server-only';
import { getMe, type HouseholdMe } from '@cooquoi/api-client';
import { cache } from 'react';
import { createApiClientWithToken } from './create-api-client';

/**
 * `GET /v1/me`, shared per request: the locale lookup and the layout both need it, and an
 * API that is down must cost one timeout, not one per caller. Keyed by token (a string) so
 * `cache()` can match calls.
 */
export const fetchMe = cache(
  async (accessToken: string): Promise<HouseholdMe> => {
    const { data } = await getMe({
      client: createApiClientWithToken(accessToken),
      throwOnError: true,
    });
    return data;
  },
);
