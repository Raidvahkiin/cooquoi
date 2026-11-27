# E02.S04 — IngredientStock aggregate: restock and put away

## Goal

A household member restocks several ingredients in one request (S1). Every item starts with no pantry ("to put away"), with its entered quantity kept, a normalized quantity when a path exists, and an expiry proposed by the effective kind. The member then puts an item away into a Pantry aggregate, or moves it on, whole or split by a quantity (S2). The expiry is recomputed for the target storage.

## Acceptance

- [x] S1 `POST /v1/stock:restock` (Idempotency-Key, no If-Match): all lines in one transaction; one IngredientStock aggregate per `(household, ingredient)` (created on first restock); `ItemsRestocked`. FV3: every item has no pantry; a retired ingredient → 422 `foodventory.ingredientRetired`. FV4: amount > 0, ≤ 3 decimals, a known unit. The response returns each item with its normalized quantity (or unnormalized + a conversion proposal: the Foodpedia default or none), proposed expiry and `expiryMissing`.
- [x] FV6: normalization order universal → household conversion (none yet) → Foodpedia P2 → unnormalized, never guessed.
- [x] FV8 at restock: `printed` gets no expiry and `expiryMissing`. `estimated` gets `addedOn` + Foodpedia shelf life (proposed). `longLife` gets none and is not flagged. A typed date is `manual`, and a past date is allowed.
- [x] S2 `POST /v1/stock-items/{id}:move` (`pantryId` required, optional `amount` + `unit`, optional `expiry`): FV2 (another household's pantry or item → 404). FV9: no move back to none; a split creates a new item with the moved quantity in the entered unit, keeping `source` and `addedOn`. A quantity equal to remaining is a whole move; above it → 422 `foodventory.quantityExceedsRemaining`; a unit with no path → 422 `foodventory.unitNotConvertible` with ingredient and unit in `params`. FV8: a `proposed` expiry is recomputed for the target storage, a `manual` one is never touched. `StockItemMoved` carries from (none) / to / quantity / new item id.
- [x] FV13 now counts real stock items: deleting a Pantry aggregate that holds one → 422 `foodventory.notEmpty`. Q1 `itemCount` and `toPutAwayCount` are real. **Required to close the story**: the S03 stubs (`PantryContentsLookup` returning 0, literal counts in `ListPantriesEndpoint`, `// E02.S04.T05` markers) are gone, checked by the story review.
- [x] Domain tests named `FV3_…` to `FV9_…`; integration tests per endpoint; drift `none`; error-catalogue test green.

## Context

- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "IngredientStock (aggregate root)" to "Expiry", FV2–FV9, "Domain services", "Persistence notes"
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) S1, S2 and the paragraph after the stock table
- `dotnet/src/Cooquoi.Context.Foodpedia/PublicApi/` (from E02.S01.T04)
- `dotnet/src/Cooquoi.SharedKernel/Unit.cs:36-44`
- Open questions Q4, Q8 ([epic](../epic.md#open-questions))

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-contract-restock-move.md) | Contract: restock, move, stock item view and errors | done | E02.S03 | yes | subagent t01 |
| [T02](T02-ingredient-stock-domain.md) | IngredientStock aggregate: restock, move and split, normalization, expiry proposal | done | E02.S03 | yes | session s04-t02 |
| [T03](T03-persistence-foodpedia-adapter.md) | Persistence and the Foodpedia adapter | done | T02 | no | subagent t03 |
| [T04](T04-restock-endpoint.md) | Restock endpoint (S1) | done | T01, T03 | no | subagent t04 |
| [T05](T05-move-endpoint.md) | Move endpoint (S2), FV13 and real pantry counts | done | T04 | no | subagent t05 |
| [R01](R01-review-fixes.md) | Review fixes from the T01–T03 review: one `expires_on` mapping, batch Foodpedia P1/P2, shared code column length | done | T05 | no | subagent r01 |

T01 (TypeSpec, webapp messages) and T02 (`Domain/IngredientStocks/`) touch disjoint files.

## Summary

Done. Commits: T01 5d35b17, T02 6ead408, shared value objects 7d3df12, T03 0e4012b, T04 6195e55, T05 6e82742, R01 ae6b0c2 (review fixes 1–12), and 0a6091f (story-review fix: a no-op move kept reporting `Version + 1` and returned 500).
The IngredientStock aggregate restocks items with no pantry, puts them away and moves them whole or split, normalizes through the batch Foodpedia P1/P2 (2 queries), and proposes expiries per storage. A proposed expiry clears when the target storage is not the ingredient's recommended one. FV13 and Q1 count real items, and the S03 stubs are gone.
Verified: solution 273/273 before the fix, Foodventory 95/95 after it; drift none; `dotnet format` clean.
Open for S05: `expiryUnusual` is always false (Q8); no test covers the concurrent first-restock retry; restock items come back grouped by ingredient, not in line order.
