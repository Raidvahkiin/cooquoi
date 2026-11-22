import type { CommonLocale } from '@cooquoi/api-client';

export type Locale = CommonLocale;

export const LOCALES: readonly Locale[] = ['en', 'fr', 'vi'];
export const DEFAULT_LOCALE: Locale = 'en';

export function isLocale(value: string): value is Locale {
  return (LOCALES as readonly string[]).includes(value);
}
