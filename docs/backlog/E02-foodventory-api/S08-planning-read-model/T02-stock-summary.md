# E02.S08.T02 — `GetStockSummary` / `GetStock` in-process and over HTTP

## Goal

`Cooquoi.Context.Foodventory/PublicApi/` publishes `GetStockSummary(householdId, asOf, expiringWithinDays = 3)` and `GetStock(householdId, ingredientIds)`, following the Foodpedia `PublicApi/` convention. `GET /v1/stock/summary` returns the same model for the caller's household.

## Read only

- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) "Read model (in-process query)"
- [ADR 0005](../../../architecture/adr/0005-ai-suggestions-token-strategy.md) "Decision" §6
- `dotnet/src/Cooquoi.Context.Foodpedia/PublicApi/` (convention, from E02.S01.T04)
- `dotnet/src/Cooquoi.Context.Foodventory/Features/Shared/IngredientStockRows.cs`, `Features/ListIngredientStocks/` (from E02.S07.T03)
- `dotnet/AGENTS.md` "Use cases" (queries)

## Reference

- One projection, reused: build entries from `IngredientStockRows` plus two SQL aggregates per ingredient:
  - `expiringAmount`: sum of `normalized_amount` where `expires_on <= asOf + window` and the kind is not `long-life`.
  - `unknownExpiryAmount`: sum where `expires_on IS NULL` and the kind is not `long-life`.
- `code` comes from Foodpedia P1 (one call for all ids).
- `stateHash`: SHA-256 over the ordered `(ingredient_stock_id, version)` pairs of the household, hex. Any write bumps a version, so the hash follows (`AggregateSaveInterceptor`).
- In-process records are `PublicApi` types (primitives + SharedKernel). The HTTP endpoint `Features/GetStockSummary/` maps them 1:1 to the contract DTO with `asOf` = today (`TimeProvider`).
- Planning does not exist yet, so the published API is tested directly and through the endpoint.

## Skills

`dotnet-feature`, `dotnet-test`

## Model

sonnet

## Out of scope

Planning & Suggestion code; the cache itself; verification (T03)

## Subtasks

- [ ] T02.1 `PublicApi` records, `GetStockSummary`, `GetStock`, registration
- [ ] T02.2 Tests: totals with items in and out of pantries; expiring vs unknown vs long-life; unnormalized listed not summed; threshold-only ingredient listed with total 0; `stateHash` stable on read and changed after a consume; `GetStock` subset; FV2
- [ ] T02.3 `GetStockSummary` endpoint + test (same JSON as in-process for the caller)
- [ ] T02.4 Drift `none`, catalogue green; `dotnet format`

## Done when

`dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress && dotnet format dotnet/Cooquoi.slnx --verify-no-changes && pnpm nx run @cooquoi/contracts:drift 2>&1 | tail -3` passes and prints `drift: none`.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): stock summary for Planning (E02.S08.T02)`), stop.

Status: todo

Report:
