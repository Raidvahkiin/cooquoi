# E01.S07.T03 — Login, refresh, proxy and logout through the store

## Goal

The `cooquoi_session` cookie (still sealed by iron-session) holds only `{ sid }`. `startLogin`, `completeLogin`, the proxy refresh, `requireAccessToken` and `logout` read and write tokens and the pending login through the `SessionStore` (T02), with the TTLs from ADR 0019.

## Read only

- [ADR 0019](../../../architecture/adr/0019-webapp-sessions-in-redis.md) "Decision" §1-2
- `ts/apps/cooquoi-webapp/src/shared/auth/session.ts` (whole), `login.ts:16-72`, `tokens.ts:9-26`, `require-session.ts:14-15`, `index.ts:1-4`
- `ts/apps/cooquoi-webapp/src/proxy.ts:25-40`
- `ts/apps/cooquoi-webapp/test/setup.ts:6-10`, `test/cookie-jar.ts`, `test/auth.ts`
- T02 report (store API and file names)

## Reference

Today:

- `getSession()` (`session.ts:40-42`) is `getIronSession<SessionData>(await cookies(), sessionOptions())`. Callers use `session.save()` and `session.destroy()` directly:
  - `login.ts:16-22` writes `pendingLogin`.
  - `login.ts:36-53` writes `tokens`.
  - `login.ts:61-67` destroys the session.
- `proxy.ts:25-40` opens its own iron-session over `request.cookies` and `response.cookies`, refreshes the tokens through `isExpiringSoon` and `refreshTokens` (`tokens.ts:20-26`), and saves.

Target:

- Cookie data `{ sid?: string }`. Redis value `SessionData` (tokens and/or pendingLogin), unchanged.
- Helpers in `session.ts`, used by `login.ts`, `require-session.ts` and `proxy.ts` so none of them touches iron-session or Redis directly:
  - `loadSession()` returns `{ sid, data }` or nothing;
  - `saveSession(data, ttlSeconds)` creates the sid when missing;
  - `rotateSession()` issues a new sid at `completeLogin`;
  - `deleteSession()`.

  In `proxy.ts`, keep the request/response cookie variant and share the code with a small cookie-access parameter.
- TTLs:
  - pending login: 600 s;
  - signed-in session: the refresh token lifetime. Use `refresh_expires_in` from the token response, which `toAuthTokens` at `tokens.ts:9` gets. Add `refreshExpiresAt` to `AuthTokens` if needed.
  - Refresh in `proxy.ts` extends the TTL.
- A missing key, after a Redis restart or expiry, behaves like a missing cookie: redirect to login. A Redis connection error leads to the error page, never a token leak in logs.
- Tests: `vi.mock` the store module to return the in-memory store from T02, in `test/setup.ts`, so the existing route, proxy and query tests keep their shape. Add tests:
  - the cookie is under 200 bytes after `completeLogin`;
  - a new sid after login;
  - logout deletes the key;
  - an expired key redirects to login;
  - refresh extends the TTL.

## Skills

`webapp-feature`, `webapp-test`, `refactor`

## Model

opus (cross-cutting change through auth and the proxy)

## Out of scope

- The store implementations (T02); the Aspire resource (T01); running under Aspire (T04)
- API changes (none: the API stays stateless)

## Subtasks

- [x] T03.1 `session.ts`: sid cookie and store-backed helpers; update the `index.ts` barrel
- [x] T03.2 `login.ts` (start, complete with rotation, logout) and `require-session.ts` through the helpers
- [x] T03.3 `proxy.ts` refresh through the helpers, extending the TTL
- [x] T03.4 `test/setup.ts` store mock; update the existing auth tests and add the new cases above

## Done when

- `pnpm nx run-many -t typecheck lint test -p @cooquoi/webapp 2>&1 | tail -10` is green.
- `grep -rn 'getIronSession' ts/apps/cooquoi-webapp/src` matches only `session.ts`.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(webapp): server-side sessions in Redis (E01.S07.T03)`), stop.

Status: done (not committed: parent commits)

Report:
- Cookie holds only `{ sid }`; `session.ts` has `loadSession`/`saveSession`/`rotateSession`/`deleteSession` (optional `CookieAccess` for the proxy); `getSession`/`sessionOptions` removed, login, logout, `requireAccessToken`, proxy and `resolve-locale` go through them. TTLs: pending 600 s, signed-in = `refreshExpiresAt` (new on `AuthTokens`, from `refresh_expires_in`, 1800 s fallback); refresh re-SETs with the new TTL. Review fixes: `extend` removed; Redis fails fast (2 s connect timeout, 3 reconnects then a new client, no offline queue); proxy lets the page through on a store error; `startLogin` rotates the sid too.
- Tests: `test/setup.ts` mocks `session-store` with `test/session-store.ts` (memory store, reset per test); `test/auth.ts` gains `signIn`/`currentSession`; new cases: rotation, cookie size, logout deletes key, expired key redirects, refresh extends TTL.
- Surprise: a sealed `{ sid }` cookie is ~308 bytes (iron seal overhead ~260), so "under 200 bytes" is impossible; the test asserts < 400.
- Verified: typecheck, lint, test (85 node tests) green; biome-check and knip green; `getIronSession` only in `session.ts`. Store errors in the proxy propagate (error page), only a refused refresh signs out.
