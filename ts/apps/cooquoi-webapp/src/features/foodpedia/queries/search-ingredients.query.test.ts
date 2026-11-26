import { delay, HttpResponse, http } from 'msw';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { signIn } from '../../../../test/auth';
import { API_BASE_URL } from '../../../../test/fixtures/household';
import { server } from '../../../../test/msw/node';
import { searchIngredientsPage } from './search-ingredients.query';

const locale = vi.hoisted(() => ({ current: 'fr' }));
vi.mock('next-intl/server', () => ({ getLocale: async () => locale.current }));

beforeEach(async () => {
  locale.current = 'fr';
  await signIn();
});

function captureRequests() {
  const urls: URL[] = [];
  const languages: (string | null)[] = [];
  server.events.on('request:start', ({ request }) => {
    if (new URL(request.url).pathname === '/v1/ingredients') {
      urls.push(new URL(request.url));
      languages.push(request.headers.get('Accept-Language'));
    }
  });
  return { urls, languages };
}

describe('searchIngredientsPage', () => {
  it('forwards q, page, sort and the locale, and maps names in that locale', async () => {
    const { urls, languages } = captureRequests();

    const result = await searchIngredientsPage({
      q: 'saumon',
      page: 2,
      sort: '-name',
    });

    expect(urls[0]?.searchParams.get('q')).toBe('saumon');
    expect(urls[0]?.searchParams.get('page')).toBe('2');
    expect(urls[0]?.searchParams.get('sort')).toBe('-name');
    expect(languages[0]).toBe('fr');
    expect(result).toMatchObject({
      page: 2,
      totalCount: 2,
      sort: { key: 'name', direction: 'descending' },
      rows: [
        { code: 'salmon', name: 'Saumon' },
        { code: 'rice', name: 'Riz' },
      ],
    });
  });

  it('returns a 400 problem as an error with its code', async () => {
    server.use(
      http.get(`${API_BASE_URL}/v1/ingredients`, () =>
        HttpResponse.json(
          {
            type: 'about:blank',
            title: 'Invalid',
            status: 400,
            code: 'common.validation',
          },
          { status: 400 },
        ),
      ),
    );

    await expect(searchIngredientsPage({ q: '', page: 1 })).resolves.toEqual({
      error: { code: 'common.validation' },
    });
  });

  it('retries without sort when the API rejects it', async () => {
    let calls = 0;
    server.use(
      http.get(`${API_BASE_URL}/v1/ingredients`, ({ request }) => {
        calls += 1;
        if (new URL(request.url).searchParams.has('sort')) {
          return HttpResponse.json(
            {
              type: 'about:blank',
              title: 'Invalid',
              status: 400,
              code: 'common.invalidSort',
            },
            { status: 400 },
          );
        }
        return HttpResponse.json({
          items: [],
          page: 1,
          pageSize: 20,
          totalCount: 0,
        });
      }),
    );

    await expect(
      searchIngredientsPage({ q: '', page: 1, sort: 'code' }),
    ).resolves.toMatchObject({ rows: [], totalCount: 0 });
    expect(calls).toBe(2);
  });

  it('returns a network failure as common.unexpected', async () => {
    server.use(
      http.get(`${API_BASE_URL}/v1/ingredients`, () => HttpResponse.error()),
    );

    await expect(searchIngredientsPage({ q: '', page: 1 })).resolves.toEqual({
      error: { code: 'common.unexpected' },
    });
  });

  it('returns a 401 as common.unauthorized', async () => {
    server.use(
      http.get(`${API_BASE_URL}/v1/ingredients`, () =>
        HttpResponse.json(
          { type: 'about:blank', title: 'Unauthorized', status: 401 },
          { status: 401 },
        ),
      ),
    );

    await expect(searchIngredientsPage({ q: '', page: 1 })).resolves.toEqual({
      error: { code: 'common.unauthorized' },
    });
  });

  it('returns a timeout as common.unexpected', async () => {
    server.use(
      http.get(`${API_BASE_URL}/v1/ingredients`, async () => {
        await delay(1000);
        return HttpResponse.json({
          items: [],
          page: 1,
          pageSize: 20,
          totalCount: 0,
        });
      }),
    );

    await expect(searchIngredientsPage({ q: '', page: 1 })).resolves.toEqual({
      error: { code: 'common.unexpected' },
    });
  });

  it('stays bounded when the response never ends, and fails fast right after a timeout', async () => {
    server.use(
      http.get(`${API_BASE_URL}/v1/ingredients`, async () => {
        await delay('infinite');
        return HttpResponse.json({});
      }),
    );

    const started = Date.now();
    const first = await searchIngredientsPage({ q: 'a', page: 1 });
    const second = await searchIngredientsPage({ q: 'b', page: 1 });

    expect(first).toEqual({ error: { code: 'common.unexpected' } });
    expect(second).toEqual({ error: { code: 'common.unexpected' } });
    // One timeout (200 ms in tests), not two.
    expect(Date.now() - started).toBeLessThan(400);
  });
});
