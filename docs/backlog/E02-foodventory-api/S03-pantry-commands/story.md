# E02.S03 — Pantry aggregate commands and list

## Goal

A household member creates, renames, re-stores and deletes Pantry aggregates and lists them (C1–C4, Q1), through the operations declared in E02.S02.T03. Stock does not exist yet, so the item counts are 0 and FV13 cannot fail. E02.S04.T05 completes both once put-away exists.

## Acceptance

- [ ] C1 `POST /v1/pantries`: 201 + `Location`; FV1: 409 `foodventory.name-taken` (case- and accent-insensitive: "Réfrigérateur" vs "refrigerateur"), 400 `foodventory.invalid-name` / validation.
- [ ] C2 `:rename`, C3 `:set-storage` (storage or none): 204 + ETag, `If-Match` required (428 / 412), events `PantryRenamed` / `PantryStorageChanged`; renaming to the same name raises nothing.
- [ ] C4 `DELETE /v1/pantries/{id}`: 204 with `If-Match`, `PantryDeleted`. The FV13 pre-check exists (it counts stock items in the pantry, 0 for now).
- [ ] Q1 `GET /v1/pantries`: the caller's Pantry aggregates only (FV2), with `itemCount` and `toPutAwayCount`.
- [ ] FV2: another household's pantry id is a 404 on every command.
- [ ] Drift `none`; error-catalogue test green.

## Context

- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) Q1, "Commands — pantries"
- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) FV1, FV2, FV13
- `contracts/contexts/foodventory/routes.tsp` (from E02.S02.T03)
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/DefineConversion/DefineConversionEndpoint.cs:16-31`, `Features/CreateIngredient/`
- `dotnet/tests/Cooquoi.Context.Foodpedia.Tests/Features/DefineConversion/DefineConversionEndpointTests.cs`

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-create-rename-storage.md) | Create, rename and set storage | todo | E02.S02 | no | |
| [T02](T02-delete-and-list.md) | Delete and list Pantry aggregates | todo | T01 | no | |

Both tasks edit `PantryAggregate.cs`, so they run in order.

## Summary

(written when done)
