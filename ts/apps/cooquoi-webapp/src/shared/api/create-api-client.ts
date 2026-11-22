import 'server-only';
import { type Client, createClient, createConfig } from '@cooquoi/api-client';
import { requireAccessToken } from '@/shared/auth';
import { getEnv } from '@/shared/config/env';

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
      fetch: (request) =>
        globalThis.fetch(
          new Request(request, {
            signal: AbortSignal.timeout(COOQUOI_API_TIMEOUT_MS),
          }),
        ),
    }),
  );
}

/** API client for the current request, authenticated as the signed-in user (ADR 0011). */
export async function createApiClient(): Promise<Client> {
  return createApiClientWithToken(await requireAccessToken());
}
