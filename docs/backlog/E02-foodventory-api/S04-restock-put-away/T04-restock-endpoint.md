# E02.S04.T04 — Restock endpoint (S1)

## Goal

`POST /v1/stock:restock` restocks every line in one transaction for the caller's household. It returns the created items as `StockItemView`s and a conversion proposal per unnormalized line.

## Read only

- `dotnet/AGENTS.md` "Use cases", "Responses and HTTP semantics" (idempotency, concurrency)
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) S1 and the paragraph after the stock table
- `contracts/contexts/foodventory/routes.tsp` `restock` (from T01)
- `dotnet/src/Cooquoi.Context.Foodventory/Integration/{CurrentHousehold,FoodpediaFactsProvider}.cs`
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/CreateIngredient/` (request + validator + endpoint)
- `dotnet/tests/Cooquoi.Context.Foodpedia.Tests/Features/DefineConversion/DefineConversionEndpointTests.cs:31-46` (Idempotency-Key, tracked session)

## Reference

- Endpoint `Features/Restock/RestockEndpoint.cs`: `[WolverinePost("/v1/stock:restock")]`. Group lines by ingredient, get facts once for all ingredients, load the existing IngredientStock aggregates in one query, create the missing ones (`db.Add`), and call `Restock` on each. Wolverine saves once, so it is one transaction (use cases S1). An unknown ingredient id → 404 `common.not-found` with the id in `params`. Return 200 `RestockResult` (no `[EmptyResponse]`). Add `OperationContract.ActionWithResult` in `dotnet/src/Cooquoi.Platform/OpenApi/` if T01's template has no C# twin.
- Race on first restock of an ingredient: two requests both create → unique violation on `ux_ingredient_stocks_household_ingredient`. Make the concurrency retry cover it (check `ConcurrencyRetryMiddleware` in `dotnet/src/Cooquoi.Platform/`), or map it to 409 `common.concurrency-conflict`. Say which in the report.
- `StockItemView` mapping: a `StockItemViews.From(item, stock, facts, pantries)` in `Features/Shared/` (or `Persistence/Projections/`), reused by T05 and E02.S07. `ingredientName` comes from P1 in the `Accept-Language` locale.
- Validator: lines 1–50, amount > 0 (≤ 3 decimals is the domain's FV4), unit from `Unit.All` (`[ContractCodes]`).

## Skills

`dotnet-feature`, `dotnet-test`

## Model

sonnet

## Out of scope

Move (T05); household conversions in proposals (E02.S06)

## Subtasks

- [ ] T04.1 Request, validator, `ActionWithResult` contract mapping
- [ ] T04.2 Endpoint and `StockItemViews`
- [ ] T04.3 Integration tests: two lines of different ingredients and two of the same ingredient in one call; items have no pantry (FV3); `ItemsRestocked` (tracked session); printed → `expiryMissing`, estimated → proposed date, long-life → none; unnormalized line + proposal; retired → 422; same Idempotency-Key replays without duplicates; second household sees nothing (FV2)
- [ ] T04.4 Drift shows only `moveStockItem` missing

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress` is green and `pnpm nx run @cooquoi/contracts:drift 2>&1 | tail -5` lists only `moveStockItem`.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): restock endpoint (E02.S04.T04)`), stop.

Status: todo

Report:
