# E01.S03.T02 — Foodpedia models, routes and error codes

## Goal

`contracts/contexts/foodpedia/` declares Q1 search ingredients, Q2 get ingredient, C1 create ingredient and C5 define conversion through the T01 templates, exactly as the S02 backend serves them, and the generated client exposes them.

## Read only

- `contracts/common/operations.tsp`, `contracts/common/paging.tsp`, `contracts/common/headers.tsp` (written by T01)
- `contracts/contexts/household/{main,models,routes}.tsp` (layout of a context: namespace, tag, imports)
- [Foodpedia use cases](../../../contexts/foodpedia/use-cases.md) rows Q1, Q2, C1, C5
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/` — only the request/response records listed in Reference
- `.agents/skills/api-client-sync/SKILL.md` (regeneration steps)

## Reference

From T01 (commit `f5920af`): templates are `ListOperation`, `ReadOperation`, `CreateOperation`, `ActionOperation`, `ReplaceActionOperation` in `contracts/common/operations.tsp` (`Read`/`Create` clash with TypeSpec visibility templates). `Page<T>` instances emit inline (add `@friendlyName` for a named schema); a filter model of `@query` props emits as `components/parameters/<Ns>.<Filter>`.


Backend shapes to declare (operationIds already set on the endpoints):

- Q1 `GET /v1/ingredients`, `searchIngredients`: `SearchIngredients/SearchIngredients.cs:10-20` (`q` ≤100, `tags[]` all must match, `status` active|retired curators only, `sort[]`, `page`, `pageSize`); sort keys `relevance`, `name`, `code`, `-` = descending (`SearchIngredients/IngredientSearch.cs:19-24`); response `Page<IngredientSummary>` (`IngredientSummary.cs:4`: id, code, name, baseUnit, categoryTags); reads `Accept-Language`
- Q2 `GET /v1/ingredients/{ingredientId}`, `getIngredient`: `GetIngredient/GetIngredientEndpoint.cs:17-39` (`ETag`, 404); `IngredientDetail.cs:4-15` (id, code, locale, name, synonyms, baseUnit, conversions `ConversionDetail{unit, unitAmount, baseAmount}`, shelfLife? `{days, recommendedStorage}`, tags, status, version)
- C1 `POST /v1/ingredients`, `createIngredient`, curator: `CreateIngredient/CreateIngredient.cs:9-21` (code, names[{locale, primary, synonyms?}], baseUnit, tags, conversions?, shelfLife? with storage fridge|freezer|room); 201 + `Location` + `CreatedIngredient{id}` (`CreatedIngredient.cs:3`); errors `code-taken`, `name-taken` (409), `unknown-tags` (422)
- C5 `POST /v1/ingredients/{ingredientId}:define-conversion`, `defineConversion`, curator: `DefineConversion/DefineConversion.cs:6` (unit, unitAmount, baseAmount); `ReplaceAction` (If-Match, 204 + `ETag`)
- Error codes: `Domain/Ingredients/IngredientError.cs:8-45` and `Domain/Tags/TagError.cs:7-20` (all `foodpedia.*`; `foodpedia.duplicate-locale` appears in both, declare once) → `union FoodpediaErrorCode` in `contexts/foodpedia/errors.tsp`, next to `CommonErrorCode` (`contracts/common/errors.tsp:8-19`)
- Unit codes (decided: closed union): `dotnet/src/Cooquoi.SharedKernel/Unit.cs:9-21` → `union Unit` in `common/types.tsp` with the 16 codes (`g`, `kg`, `ml`, `l`, `tsp`, `tbsp`, `cup`, `piece`, `pack`, `can`, `bottle`, `slice`, `fillet`, `clove`, `bunch`, `pinch`) and `union BaseUnit` (`g`, `ml`, `piece`, `Unit.cs:25`); `baseUnit` fields use `BaseUnit`, conversion `unit` uses `Unit`. Adding a unit is a contract change (the T05 test does not cover units: say so in the report)

All paths above are under `dotnet/src/Cooquoi.Context.Foodpedia/Features/` unless they start with `Domain/` or `dotnet/`.

## Skills

`api-client-sync` (regenerate; no webapp consumer of Foodpedia yet, so it should change nothing in the webapp)

## Out of scope

- Other Foodpedia use cases (Q3–Q7, C2–C4, C6–C10, tags) — not built in the backend
- Common templates (T01: change them only to fix a template bug, and say so in the report); OpenAPI from the API (T03); webapp screens (S04)

## Subtasks

- [x] T02.0 `common/types.tsp`: `Unit` and `BaseUnit` unions
- [x] T02.1 `contexts/foodpedia/models.tsp`: summary, detail, conversion, shelf life, create request, created, define-conversion body, `IngredientFilter` (`tags`, `status`)
- [x] T02.2 `contexts/foodpedia/errors.tsp`: `FoodpediaErrorCode` union (18 codes, deduped)
- [x] T02.3 `contexts/foodpedia/routes.tsp`: the 4 operations on the templates, `/v1` prefix, curator ops documented as requiring the curator role
- [x] T02.4 `contexts/foodpedia/main.tsp` + import from `contracts/main.tsp`; compile
- [x] T02.5 Regenerate the client (`pnpm nx run @cooquoi/api-client:generate`); check `Foodpedia*` types and the 4 SDK functions exist in `ts/libs/api-client/src/generated`

## Done when

`pnpm nx run @cooquoi/api-client:generate` passes, `contracts/generated/openapi.yaml` contains the 4 Foodpedia paths with their operationIds, and `pnpm nx run-many -t typecheck lint -p @cooquoi/api-client @cooquoi/webapp` is green.

Then: tick the subtasks, write the report (≤5 lines; answer "can the templates express our operation kinds cleanly?"), set the status, commit (`feat(contracts): Foodpedia ingredients contract`), stop.

Status: done

Report:
- Templates: yes, mostly. Q1/Q2/C1/C5 map 1:1 to `ListOperation`/`ReadOperation`/`CreateOperation`/`ReplaceActionOperation`. One template bug fixed: `Read`/`Action`/`ReplaceAction` hardcoded `@path id`, but the backend routes use `{ingredientId}`, so they now take an optional `TPath` (default `{ @path id: Id }`); Foodpedia passes `alias IngredientPath`.
- Added `Unit` (16) and `BaseUnit` to `common/types.tsp` and Foodpedia models/errors/routes. `FoodpediaErrorCode` has 17 codes (13 ingredient + 5 tag, `duplicate-locale` counted once), not the 18 the task says. Client regenerated: 4 SDK functions and the `Foodpedia*` types are there; no webapp changes needed.
- Verified: generate, then typecheck and lint on api-client and webapp, all green. The OpenAPI file has the 4 paths with their operationIds.
- For T04: amounts are `decimal` (`format: decimal`; ASP.NET may emit `double`). `Accept-Language`, the `q` 100-char limit and the sort keys are only in doc comments, not parameters. `Page<IngredientSummary>` is inline. The T05 test does not cover units, so adding one means editing the contract by hand.
