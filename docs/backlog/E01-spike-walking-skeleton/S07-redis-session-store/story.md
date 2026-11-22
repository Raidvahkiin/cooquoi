# E01.S07 — Redis session store for the webapp BFF

## Goal

The webapp's `cooquoi_session` cookie carries only a session id. The tokens and the pending login live in Redis, with a TTL per session ([ADR 0019](../../../architecture/adr/0019-webapp-sessions-in-redis.md)). Redis runs as an Aspire resource, capped in memory and not persisted. The API is unchanged and stays stateless.

## Acceptance

- [ ] Redis is an Aspire resource with `maxmemory` set, an eviction policy of `volatile-ttl`, no RDB/AOF and no data volume. The webapp references it; nothing else does.
- [ ] After sign-in the cookie is well under 200 bytes, and Redis holds `session:<id>` with a TTL equal to the refresh token lifetime.
- [ ] Token refresh extends the TTL. Sign-out deletes the key and the cookie. A new sign-in gets a new id.
- [ ] Restarting Redis signs the user out cleanly: a redirect to login, not a 500.
- [ ] Tests: unit tests on an in-memory store; one integration test of the Redis store against a real Redis (Testcontainers); route and proxy tests stay green.
- [ ] Webapp `AGENTS.md` "Structure" (`auth/`) and the S04 findings open question 3 point to ADR 0019.

## Context

- [ADR 0019](../../../architecture/adr/0019-webapp-sessions-in-redis.md) Decision; [ADR 0011](../../../architecture/adr/0011-webapp-api-auth.md) §2
- `ts/apps/cooquoi-webapp/src/shared/auth/`:
  - `session.ts:6-42`
  - `login.ts:16-72`
  - `tokens.ts:9-26`
  - `require-session.ts:14-15`
- `ts/apps/cooquoi-webapp/src/proxy.ts:25-40` (its own `getIronSession` over request/response cookies, plus refresh)
- `aspire/Cooquoi.AppHost/Resources/Database.cs:8-12` (resource pattern), `Resources/Webapp.cs`

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-redis-aspire-resource.md) | Redis as an Aspire resource | todo | E01.S05 | yes | s07-t01 |
| [T02](T02-session-store.md) | Session store: interface, Redis and in-memory implementations | todo | E01.S05 | yes | s07-t02 |
| [T03](T03-sessions-through-store.md) | Login, refresh, proxy and logout through the store | todo | T02 | no | s07-t03 |
| [T04](T04-verify-and-findings.md) | Verify under Aspire, docs and findings | todo | T01, T03 | no | s07-t04 |

T01 (AppHost) and T02 (webapp) touch disjoint files and can run at the same time. The story review runs after T04 (`backlog-run`).

## Summary
