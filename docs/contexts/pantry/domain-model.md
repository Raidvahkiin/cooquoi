# Pantry — Domain model

- **Status**: Draft — open questions at the end
- **Type**: Supporting context, downstream of Foodpedia and Identity & Household, supplier of Planning & Suggestion ([context map](../../business/context-map.md))

## Purpose

Pantry is **what a household has in stock**: in which named pantries, how much, and until when. Its first customer is the suggestion engine (Planning & Suggestion, the core domain), which ranks recipes by stock coverage and expiring items ([ADR 0005](../../architecture/adr/0005-ai-suggestions-token-strategy.md)).

It answers three questions for the rest of the system:

1. *What do we have, and where?* — pantries and stock items.
2. *How much of each ingredient, in a comparable unit?* — entered quantities normalized to the Foodpedia base unit, household conversions first.
3. *What must be eaten soon, and what is running low?* — expiry dates (defaulted from Foodpedia shelf life) and thresholds.

Input is **API-first**: manual restock in MVP; receipt or product scan later (Receipt Scanning publishes `ItemsRestocked`). Cooking consumes stock when Planning publishes `MealCooked`. Pantry holds **no prices** (Market records `PricePaidRecorded`).

## Model

```mermaid
classDiagram
  direction LR

  class Pantry {
    «Aggregate root»
    PantryId id
    HouseholdId householdId
    PantryName name
    Storage? storage
    +rename(name)
    +setStorage(storage)
  }

  class IngredientStock {
    «Aggregate root»
    IngredientStockId id
    HouseholdId householdId
    IngredientId ingredientId
    BaseUnit baseUnit
    Threshold? threshold
    +restock(lines)
    +consume(quantity)
    +consumeItem(itemId, quantity)
    +discardItem(itemId, reason)
    +adjustItem(itemId, remaining)
    +setExpiry(itemId, expiry)
    +moveItem(itemId, pantryId)
    +setThreshold(threshold)
    +defineConversion(unit, unitAmount, baseAmount)
    +removeConversion(unit)
    +renormalize(foodpediaConverter)
  }

  class StockItem {
    «Entity»
    StockItemId id
    PantryId pantryId
    Quantity entered
    decimal remaining
    decimal? normalized
    ExpiryDate? expiry
    DateOnly addedOn
    StockSource source
  }

  class Quantity {
    «Value object»
    decimal amount
    UnitCode unit
  }

  class ExpiryDate {
    «Value object»
    DateOnly date
    ExpirySource source
  }

  class Threshold {
    «Value object»
    decimal? low
    decimal? over
  }

  class HouseholdConversion {
    «Value object»
    UnitCode unit
    decimal unitAmount
    decimal baseAmount
  }

  IngredientStock "1" *-- "0..*" StockItem : items
  IngredientStock "1" *-- "0..*" HouseholdConversion : conversions
  IngredientStock "1" *-- "0..1" Threshold
  StockItem *-- Quantity : entered
  StockItem *-- "0..1" ExpiryDate
  StockItem ..> Pantry : pantryId
```

### Aggregate boundaries

Two aggregates, both owned by a `HouseholdId`:

- **Pantry** — the named storage ("Fridge", "Freezer", "Mum's cellar"). Small, rarely changed.
- **IngredientStock** — everything one household holds of **one ingredient**, across all its pantries: the stock items, the threshold and the household conversions. One per `(householdId, ingredientId)`, created on first restock, threshold or conversion.

Why `IngredientStock` and not one `Pantry` aggregate holding every item, nor `StockItem` as its own aggregate:

| Option | Concurrency | Invariants |
|---|---|---|
| Pantry holds its items | One ETag for the whole fridge: a receipt import, a cooked meal and a manual edit all conflict. Items of one ingredient spread over several pantries. | Thresholds and totals are per ingredient, not per pantry — they would span aggregates. |
| StockItem alone | No conflicts, but none where they matter either: two concurrent consumptions of the same ingredient can both take the same item. | Consumption across items, totals, thresholds, low-stock transitions and re-normalization after a conversion all span aggregates. |
| **IngredientStock** (chosen) | Conflicts only between writes on the **same ingredient** of the same household — exactly where they must be serialized. | Every rule except pantry naming is local: totals, consumption order (P7), thresholds (P11), conversions and re-normalization (P6). |

A cooked meal or a receipt touches several ingredients, so one command may change several `IngredientStock`s in **one transaction** (same context, same database — [ADR 0010](../../architecture/adr/0010-modular-monolith-persistence.md)); a concurrency conflict retries the whole message.

### Pantry (aggregate root)

| Field | Type | Notes |
|---|---|---|
| `id` | `PantryId` | UUID v7. |
| `householdId` | `HouseholdId` | Owner ([context map, Ownership](../../business/context-map.md#rules-across-contexts)). |
| `name` | `PantryName` | Trimmed, 1–60 chars, unique per household (P1). |
| `storage` | `fridge` \| `freezer` \| `room` \| none | Same values as Foodpedia's recommended storage; used to propose a pantry and an expiry (see *Expiry defaults*). |

A first pantry ("Fridge", in the creator's language, fallback English) is created when `HouseholdCreated` is received, so J0 lands on an empty fridge.

### IngredientStock (aggregate root)

| Field | Type | Notes |
|---|---|---|
| `id` | `IngredientStockId` | UUID v7. |
| `householdId`, `ingredientId` | ids | Unique together. The ingredient is referenced by id only. |
| `baseUnit` | `BaseUnit` | Copied from Foodpedia; updated on `BaseUnitChanged`. |
| `items` | `StockItem[]` | Items currently held (remaining > 0). Depleted or discarded items leave the aggregate; events keep the history. |
| `conversions` | `HouseholdConversion[]` | The household's own ratios for this ingredient, applied before Foodpedia defaults. |
| `threshold` | `Threshold?` | Low and over levels in the base unit. |

**Total** = sum of `normalized` over normalized items. Unnormalized items are listed beside the total, never added to it.

### StockItem (entity)

| Field | Notes |
|---|---|
| `pantryId` | Where it is. Changes only by a move. |
| `entered` | **Entered quantity** as typed (`2 pack`). Never overwritten (P4). |
| `remaining` | What is left, **in the entered unit** (`1.5 pack`). Consumption and adjustment change only this. |
| `normalized` | `remaining` in the base unit, or none when **unnormalized**. Derived, stored for queries, recomputed whenever a conversion or the base unit changes (P6). |
| `expiry` | `ExpiryDate?` with its source: `proposed` (from shelf life) or `manual`. |
| `addedOn` | Restock date; tie-breaker for consumption order. |
| `source` | `manual` \| `receipt` (+ source reference for idempotency, P13). |

Keeping `remaining` in the entered unit means re-normalization is always exact: Pantry recomputes from the user's own unit and never stores a converted value it cannot reproduce.

### Quantity (value object)

`amount` (decimal, > 0, at most 3 decimals) and `unit` (a code from Foodpedia's unit catalogue, custom units included). Fractions are allowed in every unit (`0.5 piece` onion, `1.5 pack`).

### Normalization

For a quantity of an ingredient, in this order:

1. Unit **is** the base unit, or universally convertible to it (`kg` → `g`) → universal factor.
2. A **household conversion** for the unit → its ratio (possibly through a universal step: `cup` with a household `ml` conversion).
3. Foodpedia `NormalizeQuantity` (published API P2) → default conversion.
4. Otherwise → **unnormalized**. Never guessed.

When an unknown unit is entered, the restock response carries a **conversion proposal** for the client to show (MVP: the Foodpedia default if any, otherwise none; AI proposals later). Confirming it is a separate command (*define household conversion*).

### Expiry defaults

At restock, an item without an explicit expiry gets `ShelfLifePolicy.Propose(ingredientId, addedOn)` (Foodpedia P3) as a `proposed` expiry, **only if** the target pantry's storage is unset or equals the recommended storage. Otherwise (salmon put in the freezer) no expiry is proposed — the user may set one. Shelf life without a value → no expiry. Changing Foodpedia shelf life later never touches existing items.

Restock also proposes a **pantry** when none is given: the household's first pantry whose storage matches the recommended storage, else its first pantry.

### Threshold (value object)

`low` and `over`, each optional, in the base unit. **Low stock** when total < low; **overstock** when total > over. Compared on the total of all pantries (P11).

## Invariants

| # | Rule |
|---|---|
| P1 | A pantry name is 1–60 chars after trimming and unique within its household (case- and accent-insensitive). |
| P2 | Every pantry and ingredient stock belongs to one household; a stock item is only placed in, or moved to, a pantry of the same household. Callers act only on their own household. |
| P3 | Restock requires an **active** Foodpedia ingredient. Items of an ingredient retired later stay, flagged. |
| P4 | An entered quantity has amount > 0 and a known unit, and never changes after restock. |
| P5 | `remaining` is ≥ 0 and never negative; an item reaching 0 is depleted and leaves the aggregate. |
| P6 | `normalized` follows the normalization order above; no path → unnormalized. Re-normalization is triggered by household conversion changes, Foodpedia `ConversionDefined` / `ConversionRemoved` and `BaseUnitChanged`. |
| P7 | Consuming by ingredient takes from items **earliest expiry first** (no expiry last, then oldest `addedOn`). A normalizable quantity consumes normalized items; an unnormalizable one only items entered in the same unit. A quantity beyond what is held consumes everything and reports the **shortfall**. |
| P8 | Expiry is optional; proposed from shelf life as above; a manual expiry replaces it. A past date is allowed (the item is *expired*). |
| P9 | A threshold's `low` and `over` are ≥ 0 when set, and `over` > `low` when both are set. |
| P10 | A household conversion has `unitAmount` and `baseAmount` > 0, at most one per unit, and none for a unit universally convertible to the base unit (as Foodpedia F7). |
| P11 | Low stock / overstock are evaluated on the normalized total; `StockRunningLow` / `StockOverstocked` are raised only when the state **changes**. |
| P12 | A pantry can be deleted only when it holds no stock item and is not the household's last pantry. |
| P13 | A restock or consumption coming from an event (receipt line, cooked meal) is applied once per source reference. |

Errors (`ProblemDetails` codes): `pantry.name-taken` (Conflict), `pantry.not-empty`, `pantry.last-pantry`, `pantry.ingredient-retired`, `pantry.unit-not-convertible`, `pantry.invalid-threshold`, `pantry.conversion-not-needed` (BusinessRule), not found through the common code.

## Domain services

| Service | Purpose |
|---|---|
| `StockNormalizer` | Applies the normalization order: universal factors and household conversions locally, then Foodpedia `NormalizeQuantity`. Used by restock, consumption and re-normalization. |
| `ExpiryProposer` | Pantry storage check + Foodpedia `ProposeShelfLife`. |
| `ExpiryWatch` *(scheduled, daily)* | Raises `StockItemExpiringSoon` once per item entering the warning window. |

Foodpedia is called **in-process** through its published API; Pantry never reads Foodpedia tables.

## Domain events

| Event | When | Consumers |
|---|---|---|
| `PantryCreated` / `PantryRenamed` / `PantryStorageChanged` / `PantryDeleted` | Pantry lifecycle | — |
| `StockItemAdded` | Each item of a restock (manual or receipt) | Planning (suggestion cache) |
| `StockItemConsumed` | Remaining decreased by consumption (amount, shortfall) | Planning (cache) |
| `StockItemDiscarded` | Item removed unconsumed (reason: expired, spoiled, other) | Later: waste statistics |
| `StockItemAdjusted` / `StockItemMoved` / `StockItemExpiryChanged` | Corrections | Planning (cache) |
| `StockItemExpiringSoon` | Item enters the warning window | Planning (suggestions), notifications later |
| `StockRunningLow` / `StockOverstocked` | Threshold state changes (P11) | Planning (shopping list) |
| `HouseholdConversionDefined` / `HouseholdConversionRemoved` | Household conversion changed | — |

Consumed: `HouseholdCreated` (Identity), `BaseUnitChanged`, `ConversionDefined`, `ConversionRemoved`, `IngredientRetired` / `IngredientRestored` (Foodpedia), `ItemsRestocked` (Receipt Scanning, later), `MealCooked` (Planning, later).

## Persistence notes

- Schema `pantry`; tables `pantries`, `ingredient_stocks` (unique `household_id, ingredient_id`), `stock_items` (child of ingredient stock, with `pantry_id`, `normalized_amount`, `expires_on`), household conversions and threshold as owned/JSON columns.
- Indexes for the read side: `stock_items (household_id, expires_on)`, `(household_id, pantry_id)`.
- Optimistic concurrency (version / ETag) on both aggregates.
- Idempotency of event-driven commands (P13) through Wolverine's inbox plus the source reference stored on the item / consumption.

## Proposed decisions (2026-11-22)

| Question | Decision |
|---|---|
| Aggregates | `Pantry` + `IngredientStock` per household and ingredient; `StockItem` is an entity (accepted by the user 2026-11-22) |
| Quantities | Entered kept forever; remaining in the entered unit; normalized derived |
| Household conversions | Inside `IngredientStock`, applied before Foodpedia defaults |
| Prices | Not in Pantry (Market) |
| Default pantry | Created on `HouseholdCreated` |

## Open questions

1. **Consumption order across items** — earliest expiry first / oldest added first / caller must name the item. *Recommended*: earliest expiry first, oldest added as tie-breaker (P7).
2. **Consuming more than held** — reject (422) / clamp to zero and report the shortfall / allow negative stock. *Recommended*: clamp and report, for both manual and `MealCooked` (stock data is approximate; cooking already happened).
3. **Consumption in an unnormalizable unit** — reject / consume only items entered in the same unit / propose a conversion first. *Recommended*: same-unit items, else `pantry.unit-not-convertible`.
4. **Expiry warning window** — constant / household setting / query parameter. *Recommended*: query parameter `withinDays` (default 3) for queries; the same default in context config for `StockItemExpiringSoon`; household setting later.
5. **Threshold scope** — per household and ingredient / per pantry and ingredient. *Recommended*: per household and ingredient (what Planning shops for).
6. **Freezer and storage mismatch** — no proposed expiry / multiply shelf life by a storage factor / ignore storage. *Recommended*: no proposed expiry when storage differs; storage factors in Foodpedia later.
7. **Restock event naming** — Receipt Scanning publishes `ItemsRestocked` and Pantry publishes `StockItemAdded` / Pantry owns `ItemsRestocked` for every restock and consumes `ReceiptScanned`. *Recommended*: the second (an event named after a Pantry fact belongs to Pantry); needs a context-map edit.
8. **Moving part of an item** — whole items only / split on move. *Recommended*: whole items in MVP; split later if asked.
