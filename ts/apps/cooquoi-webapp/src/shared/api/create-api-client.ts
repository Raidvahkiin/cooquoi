import 'server-only';
import { type Client, createClient, createConfig } from '@cooquoi/api-client';
import { requireAccessToken } from '@/shared/auth';
import { getEnv } from '@/shared/config/env';

/** After a timeout the API counts as down for this long: calls fail at once instead of each waiting. */
const DOWN_REUSE_MS = 2000;
let downUntil = 0;

const timeoutError = () =>
  new DOMException('The API did not answer in time', 'TimeoutError');

/**
 * Aborts the request after `timeoutMs` and rejects at the same moment, whatever the fetch
 * does with the abort: the page must never wait longer than this (`AbortSignal.timeout` did
 * not always end a pending call under Next.js). A page load makes several calls one after the
 * other (locale, layout, search): once one has timed out, the next ones fail at once, so a
 * down API costs about one timeout per page load.
 */
function fetchWithTimeout(
  request: Request,
  timeoutMs: number,
): Promise<Response> {
  if (Date.now() < downUntil) return Promise.reject(timeoutError());
  const controller = new AbortController();
  return new Promise<Response>((resolve, reject) => {
    const timer = setTimeout(() => {
      downUntil = Date.now() + DOWN_REUSE_MS;
      const error = timeoutError();
      controller.abort(error);
      reject(error);
    }, timeoutMs);
    globalThis
      .fetch(new Request(request, { signal: controller.signal }))
      .then(resolve, reject)
      .finally(() => clearTimeout(timer));
  });
}

/**
 * API client for a known access token. Every call is bounded by `COOQUOI_API_TIMEOUT_MS`: without it
 * a stopped API leaves the page waiting on the OS connect timeout (minutes).
 */
export function createApiClientWithToken(accessToken: string): Client {
  const { API_BASE_URL, COOQUOI_API_TIMEOUT_MS } = getEnv();
  return createClient(
    createConfig({
      baseUrl: API_BASE_URL,
      auth: () => accessToken,
      fetch: (input) =>
        fetchWithTimeout(new Request(input), COOQUOI_API_TIMEOUT_MS),
    }),
  );
}

/** API client for the current request, authenticated as the signed-in user (ADR 0011). */
export async function createApiClient(): Promise<Client> {
  return createApiClientWithToken(await requireAccessToken());
}
