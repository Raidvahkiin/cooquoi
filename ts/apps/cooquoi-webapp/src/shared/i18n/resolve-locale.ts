import 'server-only';
import { type CommonLocale, getMe } from '@cooquoi/api-client';
import { headers } from 'next/headers';
import { cache } from 'react';
import { getSession } from '@/shared/auth';
import { createApiClientWithToken } from '@/shared/api';
import { DEFAULT_LOCALE, isLocale, type Locale } from './locales';

/**
 * Calls the generated `getMe` directly instead of the household feature (shared/ must not
 * import a feature) and without `createApiClient()`, which redirects to login: public pages
 * must render for a signed-out user.
 */
const profileLocale = cache(async (): Promise<CommonLocale | undefined> => {
  const session = await getSession();
  if (!session.tokens) return undefined;
  const accessToken = session.tokens.accessToken;
  try {
    const client = createApiClientWithToken(accessToken);
    const { data } = await getMe({ client, throwOnError: true });
    return data.profile.locale;
  } catch {
    return undefined;
  }
});

/** First supported language of an `Accept-Language` value; `fr-CA` falls back to `fr`. */
export function pickAcceptedLocale(
  acceptLanguage: string | null,
): Locale | undefined {
  if (!acceptLanguage) return undefined;
  const tags = acceptLanguage
    .split(',')
    .map((part, index) => {
      const [tag = '', ...params] = part.trim().split(';');
      const q = params.map((p) => p.trim()).find((p) => p.startsWith('q='));
      return { tag: tag.toLowerCase(), q: q ? Number(q.slice(2)) : 1, index };
    })
    .filter(({ q }) => q > 0)
    .sort((a, b) => b.q - a.q || a.index - b.index);
  for (const { tag } of tags) {
    const language = tag.split('-')[0] ?? '';
    if (isLocale(language)) return language;
  }
  return undefined;
}

/** Profile locale, then `Accept-Language`, then `en` (ADR 0012). */
export async function resolveLocale(): Promise<Locale> {
  const fromProfile = await profileLocale();
  if (fromProfile) return fromProfile;
  const acceptLanguage = (await headers()).get('accept-language');
  return pickAcceptedLocale(acceptLanguage) ?? DEFAULT_LOCALE;
}
