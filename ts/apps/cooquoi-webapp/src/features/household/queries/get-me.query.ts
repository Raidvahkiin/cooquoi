import 'server-only';
import { cache } from 'react';
import { fetchMe } from '@/shared/api';
import { requireAccessToken } from '@/shared/auth';
import { type MeViewModel, toMeViewModel } from '../models/me.view-model';

/** The signed-in user's profile; the API provisions it on first sign-in (ADR 0015). */
export const getMe = cache(async (): Promise<MeViewModel> => {
  return toMeViewModel(await fetchMe(await requireAccessToken()));
});
