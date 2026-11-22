# E01.S04 — Findings and open decisions

Story run on 2026-11-22. Spike answers (Astryx in Server Components, theme build, dark mode, paprika vs error red, Vitest browser mode) are in the [spike plan, Phase 4 status](../../../architecture/spike-walking-skeleton.md); this file holds the run, the review and what is left to decide.

## What was built

| Task | Commits | Result |
|---|---|---|
| T01 | c8a5179 | next-intl; locale from `GET /v1/me`, then `Accept-Language`, then `en` |
| T02 | 39a5058 | Astryx `cooquoi` theme, Be Vietnam Pro font, AppShell with the user menu, Vitest browser project |
| T03 | eea6bcb | `toApiError` / `ApiError`, `ErrorMessage` (Astryx Banner), `errors` messages in en/fr/vi |
| T04 | 8dc3050, 4b387c7 | .NET test: every contract error code has a non-empty translation in every locale |
| T05 | dfe47df, 13aa2c2, 5c91a0b, eb4965c, 661ec53, be08c57 | Q1 search at `/foodpedia`, then review and robustness fixes |
| T06 | — | Dropped, moved to the curator app (E01.S06) |
| T07 | a1b678d, d999bd0, 7332cff, de72fa7, 1cc8c73 | `dev-fr` / `dev-vi` users, end-to-end check under Aspire, phase 4 findings |

Verified: webapp typecheck, lint, test (64 tests, no flaky retry) and build; architecture tests 15/15. Under Aspire: sign-in for the three dev users, search in three languages (`saumon` / `salmon` → *Saumon*, `ca hoi` → *Cá hồi*), light and dark screenshots.

## Story review

Webapp review (all fixed in 13aa2c2):

- should-fix: a network failure in the search was not caught, so the page reached the Next error page
- should-fix: a 401 in the search threw a bare `Error`; it now returns `common.unauthorized`
- nit: page metadata was English only; now `generateMetadata` with en/fr/vi keys
- nit: `as never` casts in `ErrorMessage`; now one typed cast

.NET review of the T04 test (all fixed in 4b387c7):

- should-fix: `Leaves()` counted any JSON value; now only non-empty strings
- nit: `..` paths now go through `Path.GetFullPath` (`RepositoryPath` helper)
- nit: locales are discovered from `messages/*.json`, not hard-coded

## Found in the running system and fixed

- **API down hung the page** (no timeout on any API call, waiting for the OS connect timeout). Fixed: every call is bounded by `COOQUOI_API_TIMEOUT_MS` (default 5000); a root `error.tsx` shows the translated `common.unexpected` with a Retry button; `fetchMe` shares one `/v1/me` call between the locale lookup and the layout (this also removed the double `/v1/me` call from T01).
- **`API_TIMEOUT_MS` name collision**: it is also a Claude Code variable (900000 in agent shells), so the webapp inherited a 15 min timeout. Renamed to `COOQUOI_API_TIMEOUT_MS`.
- **Unhandled rejection on timeout** (`Cannot set property message`): the DOMException is now wrapped in a plain `Error`.
- **Search still hung after 661ec53** (`AbortSignal.timeout` did not end the pending fetch under Next.js). Fixed in be08c57: an explicit `AbortController` + timer that aborts and rejects together. Confirmed under Aspire with the API stopped: `/` and `/foodpedia?q=saumon` answer the error page in ~5.1 s, no unhandled rejection; 200 again once the API is back.
- Simple process-wide circuit breaker in `create-api-client.ts`: after a timeout, calls fail at once for 2 s, shared by all users of the webapp process. Fine for the spike; revisit (per-host breaker, metrics) when the API gets real traffic.

## Open questions at the end of the run

1. **UI design is a spike, not the product.** The table, paging and sort were chosen to try Astryx data components. The real Foodpedia screen should be **mobile-first with an animated card list** (PWA, [ADR 0007](../../../architecture/adr/0007-mobile-strategy.md)), with paging that suits cards ("load more" or infinite scroll). Only `features/foodpedia/components/` changes; the query, view model, error mapping, i18n and theme stay.
2. **HTTPS by default in dev.** Today everything is http, except Keycloak, which serves https with a self-signed certificate and sends HSTS, so browsers and curl upgrade `localhost:3000` to https after sign-in. Options: https everywhere with the .NET dev certificate (Aspire endpoints, `APP_URL`, secure cookie, `NODE_EXTRA_CA_CERTS`; trusting the certificate on Linux is partly manual), or http everywhere with HSTS off in the dev realm. Needs an ADR.
3. **Session cookie size.** `cooquoi_session` is 3,316 bytes of the ~4,096 browser limit per cookie, which cannot be raised. Options: server-side session store (cookie holds only an id; recommended for a BFF), keep fewer tokens, or chunked cookies. Needs an ADR (auth, S01 area).
4. **Dev user password.** It is an Aspire-generated secret, long and changing on restart; read it with `docker inspect` on the Keycloak container or in the dashboard. Option: a short fixed dev password as an AppHost user-secret parameter.

## Decisions (2026-11-22, after the story run)

1. **UI**: the table is a spike; the real Foodpedia screen is mobile-first with an animated card list (later epic).
2. **HTTPS everywhere in dev**: Aspire, API, webapp and Keycloak on the .NET dev certificate, secure cookies in dev; certificate trust on Linux scripted and documented. ADR to write.
3. **Server-side sessions in Redis**: the webapp cookie holds only a session id; tokens live in Redis (Aspire resource, TTL per session, `maxmemory` cap, no persistence). Redis chosen over Valkey (Redis 8 licence accepted for self-hosting) and over Postgres (TTL built in, later reuse as the OCR job queue). The Flutter app will not use it: it signs in to Keycloak with PKCE and calls the API with bearer tokens, so the API stays stateless. Sizing input: Redis is ~5–15 MB idle plus ~4 KB per session; Keycloak (0.5–1 GB) and OCR (Tesseract ~100–300 MB per job, deep-learning OCR 1–2 GB+) decide the VPS size. ADR to write; the OCR service gets its own ADR later.
4. **Dev user password**: a fixed Aspire parameter backed by AppHost user secrets with a short documented default; dev users stay Development-only.
5. **Order**: write the two ADRs, then E01.S05 (tooling and CI plus dev environment: HTTPS, fixed password, the items below), then a new E01.S07 (Redis session store), then E01.S06 (curator app).

## Left for later stories

- S05 (tooling): `next build` rewrites `next-env.d.ts`; `pnpm run nx:format` rewrites `dotnet/generated/openapi.json`; Biome import-order errors in 3 older test files; stale `ts/apps/cooquoi-webapp/.env.development.local` (old API and issuer ports).
- A `q` over 100 characters falls back to the full list silently, by design; revisit with the real search UX.
- When the profile cannot be loaded, the error page uses `Accept-Language`, not the profile locale (expected).
- Playwright e2e (later story) can reuse the scratchpad approach: Chromium, `ignoreHTTPSErrors`, `colorScheme` light/dark.
