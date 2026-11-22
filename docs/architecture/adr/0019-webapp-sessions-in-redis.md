# ADR 0019 — Server-side webapp sessions in Redis

- **Status**: Accepted
- **Date**: 2026-11-22
- **Amends**: [ADR 0011](0011-webapp-api-auth.md) §2 (session in an encrypted cookie)

## Context

[ADR 0011](0011-webapp-api-auth.md) stores the access token, refresh token and expiry in an encrypted iron-session cookie and plans a server-side store "if the session still exceeds the limit". The S04 run ([findings](../../backlog/E01-spike-walking-skeleton/S04-foodpedia-webapp/findings.md), "Open questions" 3) measured `cooquoi_session` at **3,316 bytes** of the ~4,096-byte per-cookie limit, which no browser lets us raise. One more claim or role in the Keycloak tokens breaks sign-in.

Coming constraints:

- A **Flutter** app ([ADR 0007](0007-mobile-strategy.md)) and an **OCR service** for scanning Latin-script receipts are planned.
- Production is a **small VPS** (Coolify, [ADR 0006](0006-cicd-github-actions-coolify.md)), so every added process must earn its memory.

| Option | Verdict |
|---|---|
| **Redis 8** | **Chosen**: per-key TTL built in, tiny footprint, first-class Aspire resource; the same instance can later back the API's distributed cache (.NET HybridCache). The Redis 8 licence (AGPLv3 / RSALv2 / SSPLv1) is accepted for self-hosting |
| Valkey | Rejected: equivalent today, but the Redis licence is no obstacle for self-hosting and Redis has the wider client and Aspire support |
| Postgres table | Rejected: no built-in expiry (needs a sweeper job), and session reads on every request load the main database |
| Fewer tokens in the cookie | Rejected: already trimmed to access token, refresh token and expiry; the access token alone grows with roles and claims |
| Chunked cookies | Rejected: more bytes on every request, fragile with partial writes, still ships tokens to the browser |

## Decision

### 1. The cookie holds only a session id

- The webapp cookie (`cooquoi_session`) carries an opaque, random **session id** (≥128 bits), still sealed by iron-session (encrypted and signed, password list for rotation), `httpOnly`, `Secure`, `SameSite=Lax` ([ADR 0018](0018-https-everywhere-in-dev.md)).
- The tokens (access token, refresh token, expiry) and the pending login (PKCE verifier, state, return path) live in **Redis** under `session:<id>`. The value is **encrypted** with `SESSION_PASSWORD` (iron-session `sealData`) before it is written, so a Redis dump, `MONITOR` or a leaked connection shows no usable token. The browser never sees a token, as in ADR 0011.
- `getSession()` keeps its signature for callers: it reads the id from the cookie, loads the tokens from Redis and refreshes them as today; sign-in writes a new session (new id, so no fixation), sign-out deletes the key and the cookie.
- The store sits behind a small interface in the webapp's `shared/auth`, so tests use an in-memory store and the storage engine can change without touching callers.

### 2. Redis as a cache, not a database

- An **Aspire resource** in development; a Coolify service on the internal network in production, never exposed publicly.
- **TTL per session**: a pending login expires after a few minutes; a signed-in session expires with the refresh token's lifetime (Keycloak SSO idle/max), and each refresh extends it.
- **`maxmemory`** capped (e.g. 64 MB) with an eviction policy of `volatile-ttl`; **no persistence** (no RDB, no AOF): a Redis restart signs everyone out, which is acceptable.
- **One ACL user per client**: the default user is disabled; each client connects with its own user, restricted to its key prefix and the commands it needs (users in a mounted `users.acl`, passwords as Aspire secret parameters). The webapp user gets `~session:*` with `get`, `set`, `expire`, `del` only.

### 3. Sharing the instance

- **Evictable data only.** Sessions and, later, the API's distributed cache (`cache:*`, its own ACL user, so the API can never read session tokens) share this instance: both may be lost or evicted. When the cache arrives, raise `maxmemory` (128–256 MB) and watch evictions so cache entries do not push sessions out.
- **Not the OCR job queue.** A queue needs `noeviction` and persistence, which conflict with this instance's settings; it gets its own Redis instance (~5–15 MB idle), decided in the OCR ADR.

### 4. Who uses these sessions

- Only the **webapp BFF**. The **Flutter app does not**: it signs in to Keycloak with Authorization Code + PKCE as the public `cooquoi-mobile` client and calls the API with bearer tokens.
- The **API stays stateless** (ADR 0011 §1): it never reads Redis for authentication.

### 5. Sizing

- Redis needs about **5–15 MB idle plus ~4 KB per session**: a few thousand sessions fit in tens of MB.
- The VPS size is set by **Keycloak (0.5–1 GB)** and the **OCR service** (Tesseract ~100–300 MB per job, deep-learning OCR 1–2 GB+), not by Redis.
- The OCR service (engine, queue, memory budget, isolation) gets **its own ADR** before it is built.

## Consequences

- The cookie shrinks to well under 200 bytes and stops growing with tokens; adding claims or roles no longer risks sign-in.
- Sessions can be revoked server-side (delete the key), which a cookie-only session could not do.
- One more process to run and monitor; if Redis is down, signed-in pages fail until it is back (sessions are lost on restart, by design).
- iron-session stays, only as the cookie sealer; `SESSION_PASSWORD` and its rotation are unchanged.
- The webapp tests need an in-memory store; one integration test runs against a real Redis.
- ACL users add one file and one secret per client; a client with the wrong prefix fails loudly (`NOPERM`), which the integration test covers.
