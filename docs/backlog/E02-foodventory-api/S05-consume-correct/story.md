# E02.S05 — Consume, discard, adjust, set expiry

## Goal

A household member consumes an ingredient (earliest expiry first, clamped, shortfall reported) or one item, discards an item, corrects its remaining quantity, and sets or clears its expiry (S3–S7).

## Acceptance

- [ ] S3 `POST /v1/stock/{ingredientId}:consume` (`amount`, `unit`, `pantryId?`; Idempotency-Key, no If-Match), FV7:
  - Takes items in order of earliest expiry first (no expiry last), oldest `addedOn` as tie-breaker, with or without a pantry; `pantryId` limits it to one Pantry aggregate.
  - A quantity beyond what is held consumes everything and returns the shortfall.
  - A unit with no path → 422 `foodventory.unitNotConvertible`, except on an item in its exact entered unit.
  - Returns the consumed items and the shortfall; raises one `StockItemConsumed` per item.
- [ ] S4 `POST /v1/stock-items/{id}:consume` (`amount`, `unit?` = entered unit): FV5, and a depleted item (0) leaves the aggregate.
- [ ] S5 `:discard` (`reason`: `expired` | `spoiled` | `other`): the item leaves; `StockItemDiscarded`.
- [ ] S6 `:adjust` (`remaining` in the entered unit, If-Match): may exceed the entered amount; 0 depletes; `StockItemAdjusted`; FV4 entered quantity unchanged.
- [ ] S7 `:set-expiry` (`date` or none, If-Match): source becomes `manual`; any kind; past date allowed (FV8); `StockItemExpiryChanged`.
- [ ] Domain tests `FV5_…`, `FV7_…`, `FV8_…`; integration tests per endpoint; FV2 isolation; drift `none`.

## Context

- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "StockItem (entity)", "Normalization" (last paragraph), FV4–FV8
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) S3–S7
- `dotnet/src/Cooquoi.Context.Foodventory/Domain/IngredientStocks/` and `Features/MoveStockItem/` (from E02.S04)

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-contract-consume-correct.md) | Contract: consume, discard, adjust, set expiry | todo | E02.S04 | yes | |
| [T02](T02-domain-consume-correct.md) | Domain: consumption order, shortfall, discard, adjust, set expiry | todo | E02.S04 | yes | |
| [T03](T03-consume-endpoints.md) | Consume endpoints (S3, S4) | todo | T01, T02 | no | |
| [T04](T04-correction-endpoints.md) | Discard, adjust and set-expiry endpoints (S5–S7) | todo | T03 | no | |

T03 and T04 both regenerate `dotnet/generated/openapi.json`, so they run in order.

## Summary

(written when done)
