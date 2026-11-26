# E02.S03 — Pantry aggregate commands and list

## Goal

A household member creates, renames, re-stores and deletes Pantry aggregates and lists them (C1–C4, Q1), through the operations declared in E02.S02.T03. Stock does not exist yet, so the item counts are 0 and FV13 cannot fail. E02.S04.T05 completes both once put-away exists.

## Acceptance

- [x] C1 `POST /v1/pantries`: 201 + `Location`; FV1: 409 `foodventory.name-taken` (case- and accent-insensitive: "Réfrigérateur" vs "refrigerateur"), 422 `foodventory.invalid-name` (decided in S02 review) / 400 validation.
- [x] C2 `:rename`, C3 `:set-storage` (storage or none): 204 + ETag, `If-Match` required (428 / 412), events `PantryRenamed` / `PantryStorageChanged`; renaming to the same name raises nothing.
- [x] C4 `DELETE /v1/pantries/{id}`: 204 with `If-Match`, `PantryDeleted`. The FV13 pre-check exists (it counts stock items in the pantry, 0 for now).
- [x] Q1 `GET /v1/pantries`: the caller's Pantry aggregates only (FV2), with `itemCount` and `toPutAwayCount`.
- [x] FV2: another household's pantry id is a 404 on every command.
- [x] Drift `none`; error-catalogue test green.

## Context

- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) Q1, "Commands — pantries"
- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) FV1, FV2, FV13
- `contracts/contexts/foodventory/routes.tsp` (from E02.S02.T03)
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/DefineConversion/DefineConversionEndpoint.cs:16-31`, `Features/CreateIngredient/`
- `dotnet/tests/Cooquoi.Context.Foodpedia.Tests/Features/DefineConversion/DefineConversionEndpointTests.cs`

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-create-rename-storage.md) | Create, rename and set storage | done | E02.S02 | no | subagent (sonnet) |
| [T02](T02-delete-and-list.md) | Delete and list Pantry aggregates | done | T01 | no | subagent (sonnet) |

Both tasks edit `PantryAggregate.cs`, so they run in order.

## Summary

Done 2026-11-26 in 482d0d2 (T01) and 2c9c17b (T02), plus the review fix. C1–C4 and Q1 serve the four pantries routes; drift is `none` and the drift gate has no `planned` list.
Review (sonnet): one should-fix — the commands shared `RenamePantryEndpoint.LoadAsync`; it is now `PantryLookup.LoadForHouseholdAsync`. Nits left: `PantryNameLookup` lowers in C# while the index uses Postgres `lower()` (the unique index is the safety net); the list orders by `Name`, not `NameKey`.
Code wins over the task files: the enum is `StorageType`, `HouseholdDirectory` is public for Wolverine codegen, commands carry `[Transactional(typeof(FoodventoryDbContext))]`, the list is `[NonTransactional]`.
For S04.T05: point `PantryContentsLookup` at `stock_items` and fill `itemCount` / `toPutAwayCount` (marked `// E02.S04.T05`); check that `PantryDeletedDomainEvent` is published from a deleted entity once something consumes it.
