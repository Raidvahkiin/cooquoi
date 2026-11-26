# E01.S04.T06 — Ingredient page with the curator define-conversion form (Q2 read, C5)

## Goal

`/foodpedia/[ingredientId]` shows the ingredient (Q2) and its conversions, and a curator defines a conversion (C5) through a Server Action sending `If-Match` and `Idempotency-Key`; a stale version or any problem shows a translated error by code.

## Read only

- `ts/apps/cooquoi-webapp/AGENTS.md` sections "File types", "`ActionResult`", "Examples", "Testing"
- [Foodpedia use cases](../../../contexts/foodpedia/use-cases.md) lines 19 (Q2) and 26-34 (C5, rules F6, F7)
- `ts/libs/api-client/src/generated/sdk.gen.ts:49` (`getIngredient`), `:58` (`defineConversion`); `types.gen.ts:89-95`, `:126-153` (`FoodpediaDefineConversion`, `FoodpediaIngredientDetail` with `version`), `:372-383` (`DefineConversionData.headers`)
- `contracts/common/headers.tsp:9-24` (ETag / If-Match a quoted number like `"3"`; `Idempotency-Key` uuid)
- `ts/apps/cooquoi-webapp/src/shared/actions/` and `src/shared/api/to-action-error.ts` (T03)

## Reference

- Action shape: `AGENTS.md` "Examples" (`consumeStockItem`) with `defineAction` from T03
- Query: `src/features/foodpedia/queries/search-ingredients.query.ts` (T05), with `Accept-Language`; 404 (`common.notFound`) → `notFound()`
- If-Match: `If-Match: "${version}"` from `IngredientDetail.version` (the ETag header is not typed in the client; `version` is the documented source)
- MSW handlers: `test/msw/handlers/foodpedia.ts` (T05)
- Error display: `useErrorMessage` / error component from `src/shared/ui/` (T03); unit labels `foodpedia.units.*` (T05)

## Skills

`webapp-feature`, `webapp-ui`, `webapp-test`

## Model

sonnet

## Decisions (settled at planning)

- Files in `src/features/foodpedia/`: `queries/get-ingredient.query.ts`, `models/ingredient.view-model.ts`, `schemas/define-conversion.schema.ts` (`ingredientId`, `version`, `idempotencyKey` uuid, `unit` ∈ `CommonUnit`, `unitAmount` > 0, `baseAmount` > 0), `actions/define-conversion.action.ts`, `components/ingredient-detail/`, `components/define-conversion-form/` (client leaf with `useActionState`).
- `Idempotency-Key`: generated once when the form renders (hidden field, `crypto.randomUUID()` on the server page), so a retried submit reuses it; a new key after a success (the page re-renders after `revalidatePath`).
- After success: `revalidatePath('/foodpedia/[id]')`; the new conversion and version appear.
- Stale version: API `412` `common.versionMismatch` (`{current}`, `{expected}`) → translated message + hint to reload. `403` `common.forbidden` (not a curator) → translated message. Field errors (`validation.*`, `foodpedia.invalidConversionAmount`, `foodpedia.conversionNotNeeded`) shown on the fields or the form.
- The form is shown to every signed-in user for the spike; the API enforces the `catalog-curator` role (see story open question).

## Out of scope

- Create-ingredient form (C1) and other commands (C2–C4, C6–C10); role-based hiding of the form; Playwright e2e

## Subtasks

- [ ] T06.1 `getIngredient` query + view model (conversions with unit labels) + Node tests (404 → `notFound`, `Accept-Language` sent)
- [ ] T06.2 Schema + `defineConversion` action + Node tests: sends `If-Match: "3"` and `Idempotency-Key`; 204 → `ok`; 412 → `common.versionMismatch` with params; 400 → `fieldErrors`; invalid input never calls the API
- [ ] T06.3 `IngredientDetail` + `DefineConversionForm` components + browser tests (fields by label, error text by code in French, unit select lists the translated units)
- [ ] T06.4 Route `src/app/(app)/foodpedia/[ingredientId]/page.tsx`; messages in `en` / `fr` / `vi`

## Done when

`pnpm nx run-many -t typecheck lint test -p @cooquoi/webapp 2>&1 | tail -15` is green and `pnpm nx run @cooquoi/webapp:build 2>&1 | tail -5` succeeds.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(webapp): define a conversion from the ingredient page (E01.S04.T06)`), stop.

Status: dropped (moved to the curator app, E01.S06; this brief is the starting point of its define-conversion task, to re-plan once the curator app ADR is decided)

Report:
