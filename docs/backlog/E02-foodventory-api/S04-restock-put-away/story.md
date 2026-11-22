# E02.S04 — IngredientStock aggregate: restock and put away

## Goal

A household member restocks several ingredients in one request (S1). Every item starts with no pantry ("to put away"), with its entered quantity kept, a normalized quantity when a path exists, and an expiry proposed by the effective kind. The member then puts an item away into a Pantry aggregate, or moves it on, whole or split by a quantity (S2). The expiry is recomputed for the target storage.

## Acceptance

- [ ] S1 `POST /v1/stock:restock` (Idempotency-Key, no If-Match): all lines in one transaction; one IngredientStock aggregate per `(household, ingredient)` (created on first restock); `ItemsRestocked`. FV3: every item has no pantry; a retired ingredient → 422 `foodventory.ingredient-retired`. FV4: amount > 0, ≤ 3 decimals, a known unit. The response returns each item with its normalized quantity (or unnormalized + a conversion proposal: the Foodpedia default or none), proposed expiry and `expiryMissing`.
- [ ] FV6: normalization order universal → household conversion (none yet) → Foodpedia P2 → unnormalized, never guessed.
- [ ] FV8 at restock: `printed` gets no expiry and `expiryMissing`. `estimated` gets `addedOn` + Foodpedia shelf life (proposed). `longLife` gets none and is not flagged. A typed date is `manual`, and a past date is allowed.
- [ ] S2 `POST /v1/stock-items/{id}:move` (`pantryId` required, optional `amount` + `unit`, optional `expiry`): FV2 (another household's pantry or item → 404). FV9: no move back to none; a split creates a new item with the moved quantity in the entered unit, keeping `source` and `addedOn`. A quantity equal to remaining is a whole move; above it → 422 `foodventory.quantity-exceeds-remaining`; a unit with no path → 422 `foodventory.unit-not-convertible` with ingredient and unit in `params`. FV8: a `proposed` expiry is recomputed for the target storage, a `manual` one is never touched. `StockItemMoved` carries from (none) / to / quantity / new item id.
- [ ] FV13 now counts real stock items: deleting a Pantry aggregate that holds one → 422 `foodventory.not-empty`. Q1 `itemCount` and `toPutAwayCount` are real.
- [ ] Domain tests named `FV3_…` to `FV9_…`; integration tests per endpoint; drift `none`; error-catalogue test green.

## Context

- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "IngredientStock (aggregate root)" to "Expiry", FV2–FV9, "Domain services", "Persistence notes"
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) S1, S2 and the paragraph after the stock table
- `dotnet/src/Cooquoi.Context.Foodpedia/PublicApi/` (from E02.S01.T04)
- `dotnet/src/Cooquoi.SharedKernel/Unit.cs:36-44`
- Open questions Q4, Q8 ([epic](../epic.md#open-questions))

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-contract-restock-move.md) | Contract: restock, move, stock item view and errors | todo | E02.S03 | yes | |
| [T02](T02-ingredient-stock-domain.md) | IngredientStock aggregate: restock, move and split, normalization, expiry proposal | todo | E02.S03 | yes | |
| [T03](T03-persistence-foodpedia-adapter.md) | Persistence and the Foodpedia adapter | todo | T02 | no | |
| [T04](T04-restock-endpoint.md) | Restock endpoint (S1) | todo | T01, T03 | no | |
| [T05](T05-move-endpoint.md) | Move endpoint (S2), FV13 and real pantry counts | todo | T04 | no | |

T01 (TypeSpec, webapp messages) and T02 (`Domain/IngredientStocks/`) touch disjoint files.

## Summary

(written when done)
