import { delay, HttpResponse, http } from 'msw';
import { describe, expect, it } from 'vitest';
import { API_BASE_URL } from '../../../test/fixtures/household';
import { server } from '../../../test/msw/node';
import { fetchMe } from './fetch-me';

describe('fetchMe', () => {
  it('fails with a plain Error after the timeout, and callers share one wait', async () => {
    let calls = 0;
    server.use(
      http.get(`${API_BASE_URL}/v1/me`, async () => {
        calls += 1;
        await delay(1000);
        return HttpResponse.json({});
      }),
    );

    const results = await Promise.allSettled([
      fetchMe('token-a'),
      fetchMe('token-a'),
    ]);
    const [first] = results;
    const sequential = await fetchMe('token-a').catch(
      (error: unknown) => error,
    );

    expect(first?.status).toBe('rejected');
    const error = (first as PromiseRejectedResult).reason;
    expect(error).toBeInstanceOf(Error);
    // Next.js assigns `message`: must not throw as it does on a DOMException.
    expect(() => {
      error.message = 'changed';
    }).not.toThrow();
    expect(sequential).toBeInstanceOf(Error);
    expect(calls).toBe(1);
  });
});
