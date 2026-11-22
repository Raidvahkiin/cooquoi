import 'server-only';
import { getMe, type HouseholdMe } from '@cooquoi/api-client';
import { createApiClientWithToken } from './create-api-client';

/** How long a failure is reused, so the callers of one page load share a single failed wait. */
const FAILURE_REUSE_MS = 2000;

const shared = new Map<string, Promise<HouseholdMe>>();

async function loadMe(accessToken: string): Promise<HouseholdMe> {
  try {
    const { data } = await getMe({
      client: createApiClientWithToken(accessToken),
      throwOnError: true,
    });
    return data;
  } catch (cause) {
    // A plain Error: Next.js assigns `.message` on what the render throws, which a
    // DOMException (timeout, abort) or a problem body does not allow.
    throw new Error('Could not load the profile', { cause });
  }
}

/**
 * `GET /v1/me`, shared by the locale lookup and the layout. They may run outside one React
 * `cache()` scope (next-intl config), and an API that is down must cost one timeout per page
 * load, not one per caller: in-flight calls and a fresh failure are reused, keyed by token.
 */
export function fetchMe(accessToken: string): Promise<HouseholdMe> {
  const existing = shared.get(accessToken);
  if (existing) return existing;
  const request = loadMe(accessToken);
  shared.set(accessToken, request);
  request.then(
    () => shared.delete(accessToken),
    () => {
      setTimeout(() => shared.delete(accessToken), FAILURE_REUSE_MS).unref();
    },
  );
  return request;
}
