# E01.S07.T02 — Session store: interface, Redis and in-memory implementations

## Goal

`shared/auth` has a `SessionStore` with get, set (with TTL), extend and delete, keyed by session id. It has a Redis implementation and an in-memory one, and the Redis one is tested against a real Redis. No caller uses it yet.

## Read only

- [ADR 0019](../../../architecture/adr/0019-webapp-sessions-in-redis.md) "Decision" §1-2
- `ts/apps/cooquoi-webapp/AGENTS.md` "## Structure", "## File types", "## Clean code", "## Testing"
- `ts/apps/cooquoi-webapp/src/shared/auth/session.ts:6-24` (`AuthTokens`, `PendingLogin`, `SessionData`)
- `ts/apps/cooquoi-webapp/src/shared/config/env.ts:4-23`
- `ts/apps/cooquoi-webapp/vite.config.mts:26-34` (test env vars)
- `pnpm-workspace.yaml` catalog (lines 8-28)

## Reference

- Env: add `REDIS_URL: z.url()` to `env.ts`, next to `SESSION_PASSWORD` (:12). Add a test value in `vite.config.mts:26-34`.
- Client: the `redis` package (node-redis 5) through the catalog. Use one lazily created client per process, connected on first use and cached like `getEnv()` (`env.ts:20-23`).
- Shape:
  - `session-store.ts` holds the `SessionStore` type and `getSessionStore()`, which returns the Redis store.
  - `redis-session-store.ts` stores a JSON value under `session:<id>` with `SET … EX`, and uses `EXPIRE` and `DEL`.
  - `memory-session-store.ts` holds a Map plus expiry and is used by tests.
  - Session ids come from `crypto.randomBytes(32).toString('base64url')`, in `session-id.ts` or inside the store file.
- Tests:
  - The in-memory store gets a Node unit test of TTL behaviour with fake timers.
  - The Redis store gets one Node integration test using `@testcontainers/redis` (Docker must run). Name it `*.integration.test.ts` if the node project should be able to skip it. Check how `vite.config.mts` selects files and say in the report what you chose.

## Skills

`webapp-feature` (shared code rules), `webapp-test`

## Model

opus (new building block)

## Out of scope

- Changing `getSession()`, login, proxy or logout (T03); the Aspire resource (T01)

## Subtasks

- [ ] T02.1 `REDIS_URL` in the env schema and the test env; `redis` and `@testcontainers/redis` in the catalog and the webapp `package.json`
- [ ] T02.2 `SessionStore` type, session id generation, in-memory store + unit test
- [ ] T02.3 Redis store + Testcontainers integration test (set/get, TTL set, extend, delete, missing key → `undefined`)

## Done when

`pnpm nx run-many -t typecheck lint test -p @cooquoi/webapp 2>&1 | tail -10` is green, including the Redis integration test with Docker running.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(webapp): session store with Redis and in-memory implementations (E01.S07.T02)`), stop.

Status: todo

Report:
