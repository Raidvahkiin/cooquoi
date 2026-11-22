# E01.S04.T01 — next-intl with the locale from the user profile

## Goal

The webapp renders through next-intl in `en` / `fr` / `vi`, and the active locale comes from the signed-in user's profile (`GET /v1/me` → `profile.locale`), then `Accept-Language`, then `en` (ADR 0012 §1–§2).

## Read only

- `ts/apps/cooquoi-webapp/AGENTS.md` sections "Structure", "UI, styling, text", "Testing", "Commands"
- [ADR 0012](../../../architecture/adr/0012-internationalization.md) **Decision** §1, §2 only
- `ts/apps/cooquoi-webapp/src/features/household/queries/get-me.query.ts` and `models/me.view-model.ts`
- `ts/apps/cooquoi-webapp/src/app/layout.tsx`, `src/app/account/page.tsx`, `next.config.js`
- `ts/apps/cooquoi-webapp/vite.config.mts:8-26`, `test/setup.ts`, `test/msw/node.ts`, `test/msw/handlers/identity.ts`

## Reference

- Query pattern: `src/features/household/queries/get-me.query.ts:8-12` (`cache()`, `createApiClient()`, `throwOnError: true`, view model)
- API client: `src/shared/api/create-api-client.ts:7-14`; session: `src/shared/auth/session.ts:40` (`getSession()`, no redirect) vs `require-session.ts:14` (`requireAccessToken()`, redirects)
- Contract: `CommonLocale = 'en'|'fr'|'vi'` (`ts/libs/api-client/src/generated/types.gen.ts:45`), `HouseholdUserProfile.locale` (`:219-223`)
- MSW: handlers per area in `test/msw/handlers/<area>.ts`, fixtures in `test/fixtures/<area>.ts`, registered in `test/msw/node.ts:4`; API base URL in tests is `http://api.test` (`vite.config.mts:20`)
- Test style: `src/shared/auth/require-session.test.ts:13-40`

## Skills

`webapp-feature`, `webapp-test`

## Model

sonnet

## Lookups

- "next-intl without locale in the URL with App Router: `i18n/request.ts` `getRequestConfig`, plugin `createNextIntlPlugin(path)` in `next.config.js`, `NextIntlClientProvider` in the root layout" → use the context7 docs tool (`next-intl`), ≤10 lines of the setup for the installed version.

## Decisions (settled at planning)

- Dependency: add `next-intl` to the `catalog:` in `pnpm-workspace.yaml`, then `"next-intl": "catalog:"` in the webapp `package.json`.
- Request config in `src/shared/i18n/request.ts`; locale resolver `resolveLocale()` in `src/shared/i18n/resolve-locale.ts`: if `getSession()` has a session → `getMe()`'s locale (same React `cache()` as the layout); else first supported language of `Accept-Language` (region falls back to language, `fr-CA` → `fr`); else `en`. A failing `getMe` (expired refresh) falls back to `Accept-Language`, it must not break public pages.
- `shared/` must not import a feature: put the locale lookup behind a small `shared/` function that calls the generated `getMe` directly, or move the resolver input into the layout. Pick one, say why in the report.
- `createApiClient()` stays locale-free. Reference-data queries (T05) pass `headers: { 'Accept-Language': await getLocale() }` themselves.
- Messages: `messages/{en,fr,vi}.json`, one top-level namespace per feature (`common`, `household` now). Translate the existing literal strings of `src/app/page.tsx` and `src/app/account/page.tsx` (no literal UI string left).
- `<html lang>` follows the active locale.

## Out of scope

- Astryx, theme, fonts, AppShell (T02); error code messages (T03); Keycloak `ui_locales`

## Subtasks

- [x] T01.1 Install `next-intl`, plugin in `next.config.js`, `NextIntlClientProvider` + `lang` in `src/app/layout.tsx`
- [x] T01.2 `resolve-locale.ts` + `request.ts` in `src/shared/i18n/` (+ `index.ts`)
- [x] T01.3 `messages/en.json`, `fr.json`, `vi.json`; existing pages use `getTranslations`
- [x] T01.4 MSW `test/msw/handlers/household.ts` + `test/fixtures/household.ts` for `GET /v1/me`
- [x] T01.5 Tests (`resolve-locale.test.ts`): profile locale wins; no session → `Accept-Language` (`fr-CA,fr;q=0.9` → `fr`, `de` → `en`); `/v1/me` 401 → falls back; plus a `get-me.query.test.ts`

## Done when

`pnpm nx run-many -t typecheck lint test -p @cooquoi/webapp 2>&1 | tail -15` is green and `pnpm nx run @cooquoi/webapp:build 2>&1 | tail -5` succeeds.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(webapp): next-intl with the profile locale (E01.S04.T01)`), stop.

Status: done

Report:

- Changed: next-intl (catalog), plugin + provider + `<html lang>`, `shared/i18n/` (`resolveLocale`: profile, Accept-Language, en), `messages/{en,fr,vi}.json`, pages translated, MSW household handler/fixture, tests.
- Verified: typecheck/lint/test green (28 tests), build succeeds.
- Choice: resolver calls generated `getMe` directly in `shared/i18n` (shared must not import a feature; `createApiClient` redirects when signed out), so a signed-in page makes a second `/v1/me` call besides the feature `getMe`.
