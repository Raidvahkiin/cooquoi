import { createClient, createConfig } from '@cooquoi/api-client';
import { HttpResponse, http } from 'msw';
import { describe, expect, it } from 'vitest';
import { API_BASE_URL } from '../../../test/fixtures/household';
import { server } from '../../../test/msw/node';
import { toApiError } from './to-api-error';

const problem = (body: Record<string, unknown>) => ({
  type: 'about:blank',
  title: 'Problem',
  status: 400,
  ...body,
});

describe('toApiError', () => {
  it('keeps the code and params of a problem', () => {
    const error = toApiError(
      problem({
        code: 'common.version-mismatch',
        params: { current: 3, expected: 2 },
      }),
    );

    expect(error).toEqual({
      code: 'common.version-mismatch',
      params: { current: 3, expected: 2 },
    });
  });

  it('groups field errors by field', () => {
    const error = toApiError(
      problem({
        code: 'common.validation',
        errors: [
          {
            field: 'names[0].primary',
            code: 'validation.max-length',
            params: { maxLength: 100 },
          },
          { field: 'names[0].primary', code: 'validation.required' },
          { field: 'code', code: 'validation.pattern' },
        ],
      }),
    );

    expect(error).toEqual({
      code: 'common.validation',
      fieldErrors: {
        'names[0].primary': [
          { code: 'validation.max-length', params: { maxLength: 100 } },
          { code: 'validation.required' },
        ],
        code: [{ code: 'validation.pattern' }],
      },
    });
  });

  it('maps an unknown or missing code to common.unexpected', () => {
    expect(toApiError(problem({ code: 'billing.nope' }))).toEqual({
      code: 'common.unexpected',
    });
    expect(toApiError(problem({}))).toEqual({ code: 'common.unexpected' });
    expect(toApiError(undefined)).toEqual({ code: 'common.unexpected' });
  });

  it('maps a network failure to common.unexpected', async () => {
    server.use(http.get(`${API_BASE_URL}/v1/me`, () => HttpResponse.error()));
    const client = createClient(createConfig({ baseUrl: API_BASE_URL }));

    const failure = await client
      .get({ url: '/v1/me' })
      .catch((error: unknown) => error);

    expect(toApiError(failure)).toEqual({ code: 'common.unexpected' });
  });
});
