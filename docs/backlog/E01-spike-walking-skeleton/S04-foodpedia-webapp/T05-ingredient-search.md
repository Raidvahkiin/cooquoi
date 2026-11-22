# E01.S04.T05 — Foodpedia ingredient search page (Q1)

## Goal

`/foodpedia` lets a signed-in user search ingredients (Q1) in an Astryx table with paging and sort, names shown in the user's locale, accent-free typing working in `en`, `fr` and `vi`.

## Read only

- `ts/apps/cooquoi-webapp/AGENTS.md` sections "Structure", "File types", "Examples", "Clean code", "UI, styling, text", "Testing"
- [Foodpedia use cases](../../../contexts/foodpedia/use-cases.md) lines 6-19 (actors, Q1)
- `ts/libs/api-client/src/generated/sdk.gen.ts:26` (`searchIngredients`) and `types.gen.ts:171-178` (`FoodpediaIngredientSummary`), `:238-293` (query and page response)
- `contracts/contexts/foodpedia/routes.tsp:13-20` (sort keys, default sort, `Accept-Language`)
- `ts/apps/cooquoi-webapp/src/features/household/` (as left by T02: query, view model, component + test)

## Reference

- Query: `src/features/household/queries/get-me.query.ts:8-12`; add `headers: { 'Accept-Language': await getLocale() }` (T01 decision: `createApiClient()` is locale-free)
- Search params: `q` (≤100 chars), `page` (1-based), `pageSize` (≤100), `sort` (`relevance`, `name`, `code`, `-` prefix; default `-relevance,name` with `q`, `name` without), optional `tags`, `status`; response `{ items, page, pageSize, totalCount }`
- Page shape (≈30 lines, params → query → feature component): `AGENTS.md` "Examples" (`PantryPage`)
- Component test style and render helper: `test/render.tsx` and the user-menu test (T02); MSW handler style: `test/msw/handlers/household.ts` (T01)

## Skills

`webapp-feature`, `webapp-ui`, `webapp-test`

## Model

sonnet

## Decisions (settled at planning)

- Feature `src/features/foodpedia/` (barrel `index.ts`). Files: `schemas/ingredient-search.schema.ts` (zod parsing of `searchParams`, invalid → defaults), `queries/search-ingredients.query.ts`, `models/ingredient-search.view-model.ts` (rows + paging info), `components/ingredient-search/` (search form as GET `<form>` so it works without JS; Astryx table with sortable columns and pagination as links that change `searchParams`).
- State lives in the URL (`?q=&page=&sort=`); no client fetching. `'use client'` only if an Astryx table/pagination piece needs it.
- Route `src/app/(app)/foodpedia/page.tsx`. Rows are not links yet: the ingredient page is not in S04 (curator screens are E01.S06; a user-facing Q2 page is a later story).
- A problem from the API (other than 401, handled by auth) is returned by the query as `{ error: ApiError }` (via `toApiError`, T03) instead of thrown, and the component renders `ErrorMessage` (T03) in place of the table.
- Base unit shown by its translated label (`foodpedia.units.<unit>` messages for the `CommonUnit` values). Empty result has a translated empty state.
- `common.invalid-sort` from the API → render the default sort instead of failing (schema keeps sort to the known keys).

## Out of scope

- Ingredient detail page and define-conversion (E01.S06); tags/status filters UI (keep the query support only); create-ingredient form; other use cases (Q2–Q7)

## Subtasks

- [x] T05.1 Schema of search params + view model (pure, with tests)
- [x] T05.2 `searchIngredients` query + MSW `test/msw/handlers/foodpedia.ts` + `test/fixtures/foodpedia.ts` (names in en/fr/vi, e.g. salmon / saumon / cá hồi); Node test: forwards `q`, `page`, `sort` and `Accept-Language`; a `400` problem comes back as `{ error }` with its code
- [x] T05.3 `IngredientSearch` component (form, table, sort, pagination, empty state, error state) + browser test (rows by role, sort link `aria-sort`, next page link, French labels, translated error)
- [x] T05.4 `foodpedia` namespace in `messages/{en,fr,vi}.json` incl. unit labels; route page

## Done when

`pnpm nx run-many -t typecheck lint test -p @cooquoi/webapp 2>&1 | tail -15` is green and `pnpm nx run @cooquoi/webapp:build 2>&1 | tail -5` succeeds.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(webapp): Foodpedia ingredient search (E01.S04.T05)`), stop.

Status: done

Report: Search page with form, sortable table, pagination, empty and error states; `foodpedia` messages in en/fr/vi; query, schema, view-model and component tests. Flaky test fixed (`optimizeDeps.include: react/jsx-dev-runtime`), plus a `next/font/google` stub alias and `process.env` define for the browser project. typecheck, lint, test (first run, no flaky) and build green.
