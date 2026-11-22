import {
  RedisContainer,
  type StartedRedisContainer,
} from '@testcontainers/redis';
import { createClient } from 'redis';
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest';
import type { SessionData } from './session';
import { getSessionStore } from './session-store';

// Tracks every client, the store's included, so all are closed before the container stops.
const clients = vi.hoisted(
  () => [] as { isOpen: boolean; close(): Promise<unknown> }[],
);
vi.mock('redis', async (importOriginal) => {
  const redis = await importOriginal<typeof import('redis')>();
  const createTrackedClient: typeof redis.createClient = (options) => {
    const client = redis.createClient(options);
    clients.push(client);
    return client;
  };
  return { ...redis, createClient: createTrackedClient };
});

// Same rules as the webapp user of aspire/redis/users.acl (ADR 0019 §2).
const WEBAPP_ACL = [
  '~session:*',
  'resetchannels',
  '-@all',
  '+get',
  '+set',
  '+expire',
  '+del',
  '+ping',
  '+hello',
  '+auth',
];
const data: SessionData = {
  tokens: {
    accessToken: 'secret-access-token',
    refreshToken: 'secret-refresh-token',
    expiresAt: 1_000,
    refreshExpiresAt: 2_000,
  },
};

let container: StartedRedisContainer;
let admin: ReturnType<typeof createClient>;
let webappUrl: string;

beforeAll(async () => {
  container = await new RedisContainer('redis:8.2').start();
  admin = await createClient({ url: container.getConnectionUrl() }).connect();
  await admin.sendCommand([
    'ACL',
    'SETUSER',
    'webapp',
    'on',
    '>webapp-password',
    ...WEBAPP_ACL,
  ]);
  const url = new URL(container.getConnectionUrl());
  url.username = 'webapp';
  url.password = 'webapp-password';
  webappUrl = url.toString();
  // The store connects as the restricted user: any command outside the ACL fails the tests.
  vi.stubEnv('REDIS_URL', webappUrl);
});

afterAll(async () => {
  await Promise.all(
    clients.filter((client) => client.isOpen).map((client) => client.close()),
  );
  await container?.stop();
  vi.unstubAllEnvs();
});

describe('redis session store', () => {
  const store = () => getSessionStore();

  it('reads back the session it wrote', async () => {
    await store().set({ id: 'round-trip', data, ttlSeconds: 60 });

    expect(await store().get('round-trip')).toEqual(data);
  });

  it('writes the session under session:<id> with its TTL', async () => {
    await store().set({ id: 'ttl', data, ttlSeconds: 60 });

    const ttl = await admin.ttl('session:ttl');
    expect(ttl).toBeGreaterThan(55);
    expect(ttl).toBeLessThanOrEqual(60);
  });

  it('restarts the TTL when the session is extended', async () => {
    await store().set({ id: 'extend', data, ttlSeconds: 60 });

    await store().extend('extend', 600);

    expect(await admin.ttl('session:extend')).toBeGreaterThan(590);
  });

  it('forgets a deleted session', async () => {
    await store().set({ id: 'delete', data, ttlSeconds: 60 });

    await store().delete('delete');

    expect(await store().get('delete')).toBeUndefined();
    expect(await admin.exists('session:delete')).toBe(0);
  });

  it('returns undefined for an unknown id', async () => {
    expect(await store().get('missing')).toBeUndefined();
  });

  it('stores an encrypted value, not readable JSON', async () => {
    await store().set({ id: 'sealed', data, ttlSeconds: 60 });

    const raw = await admin.get('session:sealed');
    expect(raw).not.toContain('secret-access-token');
    expect(() => JSON.parse(raw ?? '')).toThrow();
  });

  it('treats a value it cannot unseal as missing', async () => {
    await admin.set('session:tampered', 'not-a-seal');

    expect(await store().get('tampered')).toBeUndefined();
  });

  it('denies the webapp user any key outside session:*', async () => {
    const webapp = await createClient({ url: webappUrl }).connect();
    try {
      await expect(webapp.get('cache:other')).rejects.toThrow(/NOPERM/);
    } finally {
      await webapp.close();
    }
  });
});
