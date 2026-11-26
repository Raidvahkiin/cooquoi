# E02.S06 — Thresholds, household conversions, expiry kind override, remembered shelf life

## Goal

Inside the IngredientStock aggregate, a household sets a low/over threshold, defines or removes its own unit conversions, and overrides the expiry kind for its own stock (S8–S11). Low/overstock transitions raise events, and every conversion change re-normalizes the items. Manual expiries typed at put-away teach the remembered shelf life per storage (estimated only). The Foodventory context also reacts to the Foodpedia integration events that exist.

## Acceptance

- [ ] S8 `:set-threshold` (`low?`, `over?`, base unit): FV10 → 422 `foodventory.invalidThreshold`. FV12: low/over is evaluated on the normalized total, items with no pantry included. `StockRunningLow` / `StockOverstocked` are raised only on a state change, by every write that changes the total (restock, consume, adjust, discard, conversions).
- [ ] S10 `:define-conversion` / S11 `:remove-conversion`: FV11 (amounts > 0; one per unit; none for a universally convertible unit → 422 `foodventory.conversionNotNeeded`). FV6: items are re-normalized, household conversion before the Foodpedia default; removing one falls back to the Foodpedia default or unnormalized. Restock proposals and consumption use household conversions.
- [ ] S9 `:override-expiry-kind` (`kind` or none): the effective kind is override ?? Foodpedia kind; flags follow, dates untouched; `ExpiryKindOverridden`.
- [ ] FV15: a manual expiry typed at put-away into a Pantry aggregate with a storage, for effective kind `estimated`, stores `days = expiry − addedOn` when > 0, one per storage, the last one wins (`ShelfLifeRemembered`). FV8: later proposals use the remembered days first.
- [ ] Foodpedia events: `ConversionDefinedEvent` → re-normalize the household stocks of that ingredient; `IngredientExpiryKindChangedEvent` → update the copied kind (dates untouched); `IngredientRetiredEvent` → items flagged `ingredientRetired`. Each handler is idempotent.
- [ ] S8–S11 on an ingredient with no IngredientStock aggregate yet: open question Q6.
- [ ] Domain tests `FV6_…`, `FV10_…`, `FV11_…`, `FV12_…`, `FV15_…`; integration tests; drift `none`; catalogue green.

## Context

- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "Normalization", "Expiry", "RememberedShelfLife", "Threshold", FV6, FV8, FV10–FV12, FV15
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) S8–S11, "Events consumed"
- `dotnet/src/Cooquoi.Context.Foodpedia/PublicApi/Events/` (from E02.S01.T04)
- Open questions Q6, Q9 ([epic](../epic.md#open-questions))

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-contract.md) | Contract: threshold, override, household conversions and errors | todo | E02.S05 | yes | |
| [T02](T02-domain-thresholds-conversions.md) | Domain: thresholds with state transitions, household conversions, re-normalization | todo | E02.S05 | yes | |
| [T03](T03-domain-override-shelf-life.md) | Domain: expiry kind override and remembered shelf life | todo | T02 | no | |
| [T04](T04-endpoints-persistence.md) | Persistence and endpoints S8–S11 | todo | T01, T03 | no | |
| [T05](T05-foodpedia-event-handlers.md) | Handlers for Foodpedia integration events | todo | T04 | no | |

Requested steps (6) and (7) share this story: both change the same aggregate methods (`MoveItem`, `ExpiryProposer`, the normalizer) and the same migration. T01 (TypeSpec) and T02 (domain) are disjoint.

## Summary

(written when done)
