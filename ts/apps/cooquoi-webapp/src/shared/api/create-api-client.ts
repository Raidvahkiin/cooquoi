import 'server-only';
import { type Client, createClient, createConfig } from '@cooquoi/api-client';
import { requireAccessToken } from '@/shared/auth';
import { getEnv } from '@/shared/config/env';

/** API client for the current request, authenticated as the signed-in user (ADR 0011). */
export async function createApiClient(): Promise<Client> {
  const accessToken = await requireAccessToken();
  return createClient(
    createConfig({
      baseUrl: getEnv().API_BASE_URL,
      auth: () => accessToken,
    }),
  );
}
