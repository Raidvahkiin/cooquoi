# E02.S07.T03 — Ingredient stock queries Q3, Q4 and Q6

## Goal

`GET /v1/stock` (by ingredient, with totals and state), `GET /v1/stock/{ingredientId}` (detail + ETag) and `GET /v1/stock/low` work for the caller's household.

## Read only

- `dotnet/AGENTS.md` "Use cases", "Responses and HTTP semantics" (read one: ETag)
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) Q3, Q4, Q6; [domain model](../../../contexts/foodventory/domain-model.md) "IngredientStock" (Total), "Threshold"
- `dotnet/src/Cooquoi.Context.Foodventory/Features/ListStockItems/` (from T02)
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/GetIngredient/GetIngredientEndpoint.cs` (read one + ETag)
- `dotnet/src/Cooquoi.Context.Foodpedia/PublicApi/` (what name search P1 offers)

## Reference

- Q3: group `stock_items` by IngredientStock in SQL (sum `normalized_amount`, min `expires_on`, count missing expiry) and load the unnormalized items of the page in a second query. `state` comes from the stored state, or from total vs threshold if the state is not stored; keep it consistent with E02.S06.T02. Order by name as in T02.
- Q4: one IngredientStock projection + `IfMatch.SetETag(response, version)`.
- Q6: `Where(threshold set && state in (low[, over]))`.
- A shared `IngredientStockRows` projection in `Features/Shared/` serves Q3, Q6 and E02.S08.T02.

## Skills

`dotnet-feature`, `dotnet-test`

## Model

sonnet

## Lookups

- Does the Foodpedia published API (E02.S01.T04) offer a name search that returns ingredient ids for `q`? Expected: `file:line` of the method, or "no". If no, add a minimal `SearchIngredientIds(q, locale, limit)` to `Foodpedia/PublicApi` that reuses `IngredientSearch` (`Features/SearchIngredients/IngredientSearch.cs:19`), and say so in the report.

## Out of scope

The summary (E02.S08); Q2/Q5 (T02)

## Subtasks

- [ ] T03.1 `IngredientStockRows` projection
- [ ] T03.2 `ListIngredientStocks` (Q3) + tests (total sums put away and not, unnormalized listed not summed, `q` in fr, `pantryId` filter, state)
- [ ] T03.3 `GetIngredientStock` (Q4) + tests (ETag = version, 404 other household)
- [ ] T03.4 `ListLowStock` (Q6) + tests (no threshold excluded, `includeOverstock`)
- [ ] T03.5 Drift `none`, catalogue green

## Done when

`dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress && pnpm nx run @cooquoi/contracts:drift 2>&1 | tail -3` passes and prints `drift: none`.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): ingredient stock queries (E02.S07.T03)`), stop.

Status: todo

Report:
