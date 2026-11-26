import 'server-only';
import { cache } from 'react';
import { fetchMe, toApiError } from '@/shared/api';
import { requireAccessToken, requireLogin } from '@/shared/auth';
import { type MeViewModel, toMeViewModel } from '../models/me.view-model';

/** The signed-in user's profile; created by `POST /v1/me:sign-in` at login (ADR 0015). */
export const getMe = cache(async (): Promise<MeViewModel> => {
  try {
    return toMeViewModel(await fetchMe(await requireAccessToken()));
  } catch (error) {
    // 404: the user never signed in through the callback, so sign in again.
    if (toApiError((error as Error).cause).code === 'common.notFound') {
      return requireLogin();
    }
    throw error;
  }
});
