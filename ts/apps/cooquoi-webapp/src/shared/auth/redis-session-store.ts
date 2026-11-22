import 'server-only';
import { sealData, unsealData } from 'iron-session';
import { createClient } from 'redis';
import { getEnv } from '@/shared/config/env';
import type { SessionData } from './session';
import type { SessionStore } from './session-store';

type RedisClient = ReturnType<typeof createClient>;
/** Wrapped so a value that fails to unseal (iron-session returns `{}`) is told apart from an empty session. */
type SealedValue = { data?: SessionData };

let clientPromise: Promise<RedisClient> | undefined;

/** One connection per process, opened on first use. */
function getClient(): Promise<RedisClient> {
  clientPromise ??= createClient({ url: getEnv().REDIS_URL })
    // Without a listener a dropped connection crashes the process; the client reconnects on its own.
    .on('error', (error) => console.error('Redis session store:', error))
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

/** Values are sealed with `SESSION_PASSWORD`, so a dump or `MONITOR` shows no token (ADR 0019 §1). Uses only `GET`, `SET`, `EXPIRE`, `DEL` (ACL). */
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
    async extend(id, ttlSeconds) {
      await (await getClient()).expire(toKey(id), ttlSeconds);
    },
    async delete(id) {
      await (await getClient()).del(toKey(id));
    },
  };
}
