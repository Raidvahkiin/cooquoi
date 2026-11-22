# E02.S07.T02 — Stock item queries Q2 and Q5

## Goal

`GET /v1/stock-items` (by pantry or `none`, `needsExpiry`, sorted, paged) and `GET /v1/stock-items/expiring` return `StockItemView`s for the caller's household. The views carry ingredient names in the `Accept-Language` locale and, for items with no pantry, a suggested pantry.

## Read only

- `dotnet/AGENTS.md` "Use cases" (queries, query strings, locale)
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) Q2, Q5; [domain model](../../../contexts/foodventory/domain-model.md) "Expiry" (the last line: suggested pantry)
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/SearchIngredients/IngredientSearch.cs:19`, `SearchIngredientsEndpoint.cs:20-39`, `SearchIngredients.cs:13`
- `dotnet/src/Cooquoi.Context.Foodventory/Features/Shared/StockItemViews.cs` (from E02.S04.T04)
- `dotnet/src/Cooquoi.Context.Foodventory/Integration/FoodpediaFactsProvider.cs`

## Reference

- Endpoints copy `SearchIngredientsEndpoint`: a positional query record with `[FromQuery(Name=…)]`, `[AsParameters]`, a `SortMap<StockItemRow>` (`expiry` → `expires_on` nulls last, `added`, `name`), and `ToPageAsync`.
- `name` sort needs Foodpedia names, which are in another schema. Sort by name in memory only on the page, or drop the `name` key. Pick one, and say so in the report and in the contract doc (fix T01 if dropped).
- Project rows from `stock_items` joined to `ingredient_stocks` (effective kind, version) with `AsNoTracking`. Get names and the recommended storage for the page's ingredient ids in one `FoodpediaFactsProvider` call. Build views with `StockItemViews` (refactor it to take rows, not aggregates, if it takes aggregates).
- `expiryMissing` = effective kind ≠ `long-life` and no expiry. `expiryUnusual` = false (Q8).
- Q5: `expires_on <= today + withinDays`, plus `expires_on >= today` unless `includeExpired`, plus kind ≠ `long-life`. Order by `expires_on`, then `added_on`. "Today" comes from `TimeProvider`.

## Skills

`dotnet-feature`, `dotnet-test`

## Model

sonnet

## Out of scope

Q3, Q4, Q6 (T03); the summary (E02.S08)

## Subtasks

- [ ] T02.1 `StockItemRow` projection and `SortMap`
- [ ] T02.2 `ListStockItems` endpoint + tests (`pantryId=<id>`, `none` with suggested pantry, `needsExpiry`, sorts, paging, FV2)
- [ ] T02.3 `ListExpiringStockItems` endpoint + tests (window default 3, expired in/out, long-life excluded, missing expiry excluded; fake `TimeProvider` if the fixture offers one, else dates relative to today)

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress` is green. Drift lists only the T03 operations.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): stock item queries (E02.S07.T02)`), stop.

Status: todo

Report:
