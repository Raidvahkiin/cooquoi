# E02.S07 — Stock queries

## Goal

A household member reads their stock five ways: by pantry (including "to put away", `pantryId=none`), by ingredient with totals, one ingredient in detail, expiring soon, and low stock (Q2–Q6). Everything is scoped to their household and projected straight from the tables.

## Acceptance

- [ ] Q2 `GET /v1/stock-items` (`pantryId` = id or `none`, `needsExpiry?`, `sort` = `expiry` | `name` | `added`, paging) → `Page<StockItemView>`. `pantryId=none` lists only items with no pantry, each with a `suggestedPantryId`: the household's first Pantry aggregate whose storage matches the recommended storage, else none. `needsExpiry=true` lists only `printed` / `estimated` items with no expiry.
- [ ] Q3 `GET /v1/stock` (`q?`, `pantryId?`, paging) → `Page<IngredientStockView>`:
  - `total` (base unit, put away or not), `unnormalized[]`, `earliestExpiry`, `missingExpiryCount`, `state`.
  - `q` matches the ingredient name in the `Accept-Language` locale. The ids come from Foodpedia P1/search; see the T03 Lookup.
- [ ] Q4 `GET /v1/stock/{ingredientId}` → items per pantry and with none, total, threshold, household conversions, effective kind + override, remembered shelf lives, `version` + ETag.
- [ ] Q5 `GET /v1/stock-items/expiring` (`withinDays` default 3, `includeExpired` default true, paging) → ordered by expiry; never `long-life`; missing expiry excluded.
- [ ] Q6 `GET /v1/stock/low` (`includeOverstock?`, paging) → only ingredients with a threshold, in low (or over) state.
- [ ] FV2: no query returns another household's data. Sort keys go through a `SortMap`. Drift `none`.

## Context

- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) "Queries"
- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "Expiry" (suggested pantry), "Threshold", "Persistence notes" (indexes)
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/SearchIngredients/` (`IngredientSearch.cs:19`, `SearchIngredientsEndpoint.cs:20-39`, `SearchIngredients.cs:13`)
- Open questions Q5, Q7, Q8 ([epic](../epic.md#open-questions))

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-contract-queries.md) | Contract: stock queries Q2–Q6 | todo | E02.S06 | yes | |
| [T02](T02-stock-item-queries.md) | Stock item queries Q2 and Q5 | todo | T01 | no | |
| [T03](T03-ingredient-stock-queries.md) | Ingredient stock queries Q3, Q4 and Q6 | todo | T02 | no | |

T02 and T03 share the name lookup and `dotnet/generated/openapi.json`, so they run in order.

## Summary

(written when done)
