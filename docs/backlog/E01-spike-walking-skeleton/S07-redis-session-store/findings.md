# E01.S07 — Findings

Verified on 2026-11-22 under `aspire start`, signed in as `dev@cooquoi.local` (curl through the real OIDC flow, plus the browser pane for one sign-in).

## What was built

- Redis 8.2 Aspire resource `cache` (`maxmemory 64mb`, `volatile-ttl`, no persistence, default user off, ACL user `webapp` limited to `session:*`; TLS on the primary endpoint).
- Webapp session store (Redis and in-memory), values sealed with `SESSION_PASSWORD`; the cookie holds only a sealed `{ sid }` (ADR 0019).
- `loadSession`, `saveSession`, `rotateSession`, `deleteSession` in `shared/auth/session.ts`.

## Checks

| Acceptance | Result |
|---|---|
| Webapp connects over `rediss://` | Works with no change (Node trusts the dev CA through `SSL_CERT_DIR`). Sign-in, page loads and sign-out all go through Redis. |
| Cookie size | 308 bytes (value of `cooquoi_session`), down from 3,316. Not "well under 200": see open item 1. |
| `session:<sid>` with TTL = refresh lifetime | Key exists. The dev realm's refresh token lives 30 days: the TTL was 2,591,983 s right after sign-in (stored `refreshExpiresAt`), and a probe showed it between 2,591,900 and 2,592,001 s. |
| Value is encrypted | `GET` returns `Fe26.2*1*...` (iron seal); no `eyJ` token text. |
| Refresh extends the TTL | TTL lowered to 600 s, access token let expire, one request: TTL back above 100,000 s (the full lifetime). |
| Sign-out | Key deleted (`GET` empty) and cookie dropped from the jar; redirect to `/`. |
| New sign-in gets a new id | Four sign-ins gave four different ids. |
| Redis restart | `docker restart` on `cache`: a request with the old cookie redirects 307 to `/auth/login?returnTo=...` (also for `/foodpedia`), no 500. A new sign-in works right after (client reconnects). |
| Redis password not in the container (re-check after review fixes) | `docker inspect` shows no clear webapp password: only its SHA-256 in `REDIS_WEBAPP_PASSWORD_SHA256`, and the entrypoint seds it into the ACL. (The container also has Aspire's own `REDIS_PASSWORD`, which is not the webapp's and belongs to the disabled default user.) |
| Re-check after review fixes: sign-in, key, sign-out, restart | Sign-in 200, `session:<sid>` present with TTL at the full 30 days, sign-out deletes it; after `docker restart` a signed-in page gives 307 to `/auth/login?returnTo=...` in about 0.1 s and a new sign-in works. |
| Redis stopped (`docker stop`) | No hang: a page with a signed-in cookie answers in about 0.1 s with a 500 Next.js error page (`The client is offline`, from `redis-session-store.ts` `get`); `/auth/login` also gives 500. A request without a cookie still redirects to login (307). After `docker start`, the old cookie redirects to login and new sign-ins work (client reconnects). |
| Expiry | A key left to expire (TTL 100 s) vanished by itself. |

## Open items

1. **Cookie is 308 bytes, not under 200.** The iron-session seal adds about 260 bytes around a 43-character sid. Options: (a) keep iron-session (simple, 308 B is ~8% of the 4 KB limit, nothing else is in it); (b) an unsealed random sid with an HMAC signature (about 90 bytes, drops iron-session for the cookie; the id is already unguessable, the HMAC only avoids useless Redis lookups); (c) a plain random sid, no signature (about 43 bytes). **Decided 2026-11-22: (a) keep iron-session**; the acceptance now reads "under 400 bytes".
2. **Inspecting Redis in dev is awkward.** With the default user off and the `webapp` ACL allowing only `get/set/expire/del/ping/hello/auth`, `SCAN`, `TTL` and `ACL` are refused. The TTL here was read by probing `EXPIRE key N GT|LT` (which only changes the key on success) and by unsealing the stored value. **Resolved 2026-11-22:** a dev `admin` ACL user (secret `session-store-admin-password`) on the fixed plain port 6380 for Redis Insight; see the README. Also, plain `redis-cli` in the container reaches Redis on the `secondary` (plain) port 6380; port 6379 is TLS.
3. **Redis down gives a 500, not a redirect.** Fast (about 0.1 s, fail-fast store works), but the signed-in page and `/auth/login` return a 500 error page; the proxy catch does not cover the page's own `loadSession`, nor the login route. **Decided 2026-11-22: accepted for now**; Redis is expected to be up.
4. **Not checked:** behaviour under `maxmemory` pressure (eviction of sessions under `volatile-ttl`); production sizing.
