# E01.S04.T03 — Error messages and display by code

## Goal

Every `CommonErrorCode`, `FoodpediaErrorCode` and `ValidationCode` of the contract has a message in `en` / `fr` / `vi`, an API problem is mapped to `{ code, params }`, and the UI shows it translated by code (ADR 0012 §5).

## Read only

- `ts/apps/cooquoi-webapp/AGENTS.md` sections "Structure", "File types", "`ActionResult`", "Testing"
- `ts/libs/api-client/src/generated/types.gen.ts:10-90` (`CommonCommonErrorCode` :15, `CommonFieldError` :20, `CommonProblemDetails` :50, `CommonValidationCode` :84) and `:135` (`FoodpediaFoodpediaErrorCode`)
- `dotnet/src/BuildingBlocks/Raidvahkiin.Ddd/CommonError.cs:6-25`, `dotnet/src/Cooquoi.Platform/Idempotency/IdempotencyError.cs:8-18`, `dotnet/src/Cooquoi.Context.Foodpedia/Domain/Ingredients/IngredientError.cs:8-45` (English text and `{params}` of each code, to write the ICU messages)
- `ts/apps/cooquoi-webapp/src/shared/api/create-api-client.ts`, `messages/en.json` (as left by T02)

## Reference

- Error shape the UI receives: the `error` member of `ActionResult` in `AGENTS.md:71-79`, extended with `params`
- Problem shape: `CommonProblemDetails` `{ code, params?, errors?: { field, code, params? }[] }`; `params` carry ICU placeholders (`common.version-mismatch` → `{current}`, `{expected}`; `validation.max-length` → its max)
- Code counts (checked at planning): 13 common (7 in `CommonError.cs`, 4 in `IdempotencyError.cs`, `common.unexpected`, `common.unauthorized`), 17 foodpedia, 9 validation
- MSW + test style: `test/msw/handlers/household.ts` (T01), `src/shared/auth/require-session.test.ts:13-40`

## Skills

`webapp-feature`, `webapp-test`

## Model

sonnet

## Decisions (settled at planning)

- Messages live under one `errors` namespace, **nested by the dots of the code**: `common.version-mismatch` → `{"errors": {"common": {"version-mismatch": "…"}}}`, so `t(code)` works with next-intl's dot paths. T04's check flattens them the same way.
- `toApiError(error)` in `src/shared/api/` maps a Hey API error / `ProblemDetails` to `ApiError = { code, params?, fieldErrors? }` (`fieldErrors`: `Record<field, { code, params? }[]>`). Unknown or missing code → `common.unexpected`; network failure → `common.unexpected`. T05's query uses it; the curator app's `defineAction` (E01.S06) will reuse it.
- `ActionResult` / `defineAction` are **not** written here: the user app has no Server Action in S04 (no dead code). Keep the `AGENTS.md` `ActionResult` section, adding `params` to its error type so it matches `ApiError`.
- Display: `ErrorMessage` component in `src/shared/ui/` on an Astryx alert/inline message (pick the component from the T02 report), taking an `ApiError`, translating with `useTranslations('errors')` (or `getTranslations` if it stays a Server Component). Messages are written in all three languages now (no English placeholders in `fr`/`vi`).

## Out of scope

- The .NET catalogue check (T04); the search screen (T05); Server Actions (E01.S06)

## Subtasks

- [x] T03.1 `src/shared/api/to-api-error.ts` (+ export in `index.ts`)
- [x] T03.2 `errors` namespace in `messages/en.json`, `fr.json`, `vi.json`: 13 common + 17 foodpedia + 9 validation codes, ICU params
- [x] T03.3 `ErrorMessage` in `src/shared/ui/`
- [x] T03.4 Node test `to-api-error.test.ts`: problem with code and params, with field errors, unknown code, network error (MSW `HttpResponse.error()`)
- [x] T03.5 Browser test of `ErrorMessage`: `common.version-mismatch` with params renders the French text with `fr` messages; unknown code renders `common.unexpected`
- [x] T03.6 `AGENTS.md` "`ActionResult`": error type with `params`, one line saying it is shared with `ApiError`

## Done when

`pnpm nx run-many -t typecheck lint test -p @cooquoi/webapp 2>&1 | tail -15` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(webapp): error messages by code (E01.S04.T03)`), stop.

Status: done

Report: added `toApiError`/`ApiError` (shared/api), `errors` messages (13+17+9 codes, en/fr/vi) plus `common.errorTitle`, `ErrorMessage` on Astryx Banner, node + browser tests, AGENTS.md ActionResult uses ApiError. Verified: typecheck lint test green (3 tasks). Surprising: the test target is flaky on first run ("Vite unexpectedly reloaded a test"), Nx retries and passes.
