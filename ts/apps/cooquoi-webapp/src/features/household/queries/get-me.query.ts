import 'server-only';
import { getMe as fetchMe } from '@cooquoi/api-client';
import { cache } from 'react';
import { createApiClient } from '@/shared/api';
import { type MeViewModel, toMeViewModel } from '../models/me.view-model';

/** The signed-in user's profile; the API provisions it on first sign-in (ADR 0015). */
export const getMe = cache(async (): Promise<MeViewModel> => {
  const client = await createApiClient();
  const { data } = await fetchMe({ client, throwOnError: true });
  return toMeViewModel(data);
});
