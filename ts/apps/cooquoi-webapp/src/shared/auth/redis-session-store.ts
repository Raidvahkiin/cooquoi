import 'server-only';
import { sealData, unsealData } from 'iron-session';
import { createClient } from 'redis';
import { getEnv } from '@/shared/config/env';
import type { SessionData } from './session';
import type { SessionStore } from './session-store';

type RedisClient = ReturnType<typeof createClient>;
/** Wrapped so a value that fails to unseal (iron-session returns `{}`) is told apart from an empty session. */
type SealedValue = { data?: SessionData };

const CONNECT_TIMEOUT_MS = 2_000;
const MAX_RECONNECT_ATTEMPTS = 3;

let clientPromise: Promise<RedisClient> | undefined;

/**
 * One connection per process, opened on first use. A down Redis fails fast: commands are not
 * queued while disconnected, and after a few reconnect attempts the client gives up and the
 * next call opens a new one (so a Redis restart is picked up).
 */
function getClient(): Promise<RedisClient> {
  clientPromise ??= createClient({
    url: getEnv().SESSION_STORE_URL,
    disableOfflineQueue: true,
    socket: {
      connectTimeout: CONNECT_TIMEOUT_MS,
      reconnectStrategy: (retries, cause) => {
        if (retries < MAX_RECONNECT_ATTEMPTS) return 100 * 2 ** retries;
        clientPromise = undefined;
        return cause;
      },
    },
  })
    // Without a listener a dropped connection crashes the process. Only the message: never a value.
    .on('error', (error: Error) =>
      console.error('Redis session store:', error.message),
    )
    .connect()
    .catch((error: unknown) => {
      clientPromise = undefined;
      throw error;
    });
  return clientPromise;
}

const toKey = (id: string) => `session:${id}`;

// ttl 0: the seal never expires on its own, the Redis TTL decides.
const sealOptions = () => ({ password: getEnv().SESSION_PASSWORD, ttl: 0 });

async function unseal(sealed: string): Promise<SessionData | undefined> {
  try {
    const { data } = await unsealData<SealedValue>(sealed, sealOptions());
    return data;
  } catch {
    return undefined;
  }
}

/** Values are sealed with `SESSION_PASSWORD`, so a dump or `MONITOR` shows no token (ADR 0019 §1). Uses only `GET`, `SET`, `DEL` (ACL). */
export function createRedisSessionStore(): SessionStore {
  return {
    async get(id) {
      const sealed = await (await getClient()).get(toKey(id));
      return sealed === null ? undefined : unseal(sealed);
    },
    async set({ id, data, ttlSeconds }) {
      const sealed = await sealData(
        { data } satisfies SealedValue,
        sealOptions(),
      );
      await (await getClient()).set(toKey(id), sealed, {
        expiration: { type: 'EX', value: ttlSeconds },
      });
    },
    async delete(id) {
      await (await getClient()).del(toKey(id));
    },
  };
}
