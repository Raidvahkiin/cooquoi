import { HttpResponse, http } from 'msw';
import { describe, expect, it } from 'vitest';
import { signIn } from '../../../test/auth';
import { meFixture } from '../../../test/fixtures/household';
import { server } from '../../../test/msw/node';
import { requestHeaders } from '../../../test/request-headers';
import { pickAcceptedLocale, resolveLocale } from './resolve-locale';

describe('resolveLocale', () => {
  it('uses the locale of the signed-in profile over Accept-Language', async () => {
    await signIn();
    requestHeaders.set('accept-language', 'vi');

    await expect(resolveLocale()).resolves.toBe(meFixture.profile.locale);
  });

  it('reads Accept-Language when signed out, region falling back to language', async () => {
    requestHeaders.set('accept-language', 'fr-CA,fr;q=0.9');

    await expect(resolveLocale()).resolves.toBe('fr');
  });

  it('falls back to en when no language is supported', async () => {
    requestHeaders.set('accept-language', 'de');

    await expect(resolveLocale()).resolves.toBe('en');
  });

  it('falls back to Accept-Language when /v1/me fails', async () => {
    server.use(
      http.get(
        'http://api.test/v1/me',
        () => new HttpResponse(null, { status: 401 }),
      ),
    );
    await signIn();
    requestHeaders.set('accept-language', 'vi');

    await expect(resolveLocale()).resolves.toBe('vi');
  });
});

describe('pickAcceptedLocale', () => {
  it('honours quality values and skips unsupported languages', () => {
    expect(pickAcceptedLocale('de,vi;q=0.5,fr;q=0.8')).toBe('fr');
  });

  it('returns nothing without a header', () => {
    expect(pickAcceptedLocale(null)).toBeUndefined();
  });
});
