# E01.S07 — Redis session store for the webapp BFF

## Goal

The webapp's `cooquoi_session` cookie carries only a session id. The tokens and the pending login live in Redis, with a TTL per session ([ADR 0019](../../../architecture/adr/0019-webapp-sessions-in-redis.md)). Redis runs as an Aspire resource, capped in memory and not persisted. The API is unchanged and stays stateless.

## Acceptance

- [x] Redis is an Aspire resource with `maxmemory` set, an eviction policy of `volatile-ttl`, no RDB/AOF and no data volume. The default user is off, and the webapp connects as its own ACL user limited to `session:*` (ADR 0019 §2). The webapp references it; nothing else does yet.
- [x] After sign-in the cookie is under 400 bytes (308 B measured; "well under 200" was an estimate before the iron-session seal overhead was known, [findings](findings.md) item 1), and Redis holds `session:<id>` with a TTL equal to the refresh token lifetime.
- [x] The value in Redis is encrypted: `GET session:<id>` shows no token.
- [x] Token refresh extends the TTL. Sign-out deletes the key and the cookie. A new sign-in gets a new id.
- [x] Restarting Redis signs the user out cleanly: a redirect to login, not a 500.
- [x] Tests: unit tests on an in-memory store; one integration test of the Redis store against a real Redis (Testcontainers); route and proxy tests stay green.
- [x] Webapp `AGENTS.md` "Structure" (`auth/`) and the S04 findings open question 3 point to ADR 0019.

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
| [T01](T01-redis-aspire-resource.md) | Redis as an Aspire resource | done | E01.S05 | yes | s07-t01 |
| [T02](T02-session-store.md) | Session store: interface, Redis and in-memory implementations | done | E01.S05 | yes | s07-t02 |
| [T03](T03-sessions-through-store.md) | Login, refresh, proxy and logout through the store | done | T02 | no | s07-t03 |
| [T04](T04-verify-and-findings.md) | Verify under Aspire, docs and findings | done | T01, T03 | no | s07-t04 |

T01 (AppHost) and T02 (webapp) touch disjoint files and can run at the same time. The story review runs after T04 (`backlog-run`).

## Summary

Status: done

The cookie holds only a sealed `{ sid }` (308 B, down from 3,316 B). Tokens and the pending login are sealed in Redis under `session:<sid>`, with a TTL of the refresh lifetime (600 s while a login is pending). Refresh rewrites the session with a fresh TTL. Sign-out deletes the key, each login gets a new sid, and a Redis restart redirects to login.
Redis 8.2 runs as the Aspire `cache` resource: 64 MB, `volatile-ttl`, no persistence, default user off. The `webapp` ACL user is limited to `~session:*`, and only its password hash reaches the container. The webapp connects over `rediss://`.
The review fixes make the store fail fast, let the proxy tolerate store errors, rotate the sid on startLogin, and give the AppHost a health check that stops on error replies.
Decided 2026-11-22: keep the iron-session cookie (308 B) and accept a fast error while Redis is down. Added after close: a dev admin ACL user for Redis Insight, and function-oriented names for the resource. Open ([findings](findings.md)): eviction under memory pressure is not tested.

