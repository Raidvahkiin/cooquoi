# Foodventory — Use cases

- **Status**: Reviewed — decisions of 2026-11-22 applied
- Domain model: [domain-model.md](domain-model.md). Contract style: uniform reads, named actions for writes ([ADR 0009](../../architecture/adr/0009-api-contract-typespec.md)). API only; no UI in this phase.
- Every operation is scoped to the caller's household (FV2). **MVP** unless marked *later*.

## Actors

| Actor | Can |
|---|---|
| **Household member** (any signed-in user, in MVP the sole member) | Everything below for their own household |
| **Planning & Suggestion** | Read the published read model; publish `MealCooked` (*later*) |
| **Receipt Scanning** *(later)* | Publish `ReceiptScanned` |
| **Foodpedia context** | Publish ingredient changes that trigger re-normalization or new expiry flags |

## Queries

`StockItemView` = item id, ingredient id + name in locale, pantry (or none), entered, remaining, normalized + base unit or *unnormalized*, expiry + source, effective expiry kind, `expiryMissing`, `expiryUnusual`, added on; for items with no pantry a **suggested pantry**.

| # | Use case | Input | Output | Notes |
|---|---|---|---|---|
| Q1 | **List pantries** | — | `PantrySummary[]` (id, name, storage, item count) plus the count of items to put away | Small; not paged. |
| Q2 | **Stock items** | `pantryId` (a Pantry aggregate id, or `none` for "to put away"), `needsExpiry?`, `tags?`, `sort` (`expiry` \| `name` \| `added`), `page` | `Page<StockItemView>` | J2 "what I have"; `pantryId=none` is "what's still to put away"; `needsExpiry=true` lists items with missing expiry (`printed` / `estimated` only). |
| Q3 | **Stock by ingredient** | `q?`, `tags?`, `pantryId?`, `page` | `Page<IngredientStockView>` (ingredient, base unit, **total**, unnormalized items, earliest expiry, items with missing expiry, low/over state) | Total summed in the base unit, put away or not (FV6, FV12). |
| Q4 | **Get ingredient stock** | `ingredientId` | Items per pantry (and with none), total, threshold, household conversions, effective expiry kind + override, remembered shelf lives | Detail and editing. |
| Q5 | **Expiring soon** | `withinDays` (default 3), `includeExpired` (default true), `page` | `Page<StockItemView>` ordered by expiry | J2 "what expires soon"; never `longLife`; missing expiry excluded. |
| Q6 | **Low stock** | `includeOverstock?`, `page` | `Page<IngredientStockView>` in low (or over) state | Only ingredients with a threshold. |

## Commands — pantries

| # | Use case | Input | Rules | Event |
|---|---|---|---|---|
| C1 | **Create pantry** | `name`, `storage?` | FV1 | `PantryCreated` |
| C2 | **Rename** (`:rename`) | `name` | FV1 | `PantryRenamed` |
| C3 | **Set storage** (`:set-storage`) | `storage` or none | — | `PantryStorageChanged` |
| C4 | **Delete pantry** | — | FV13 | `PantryDeleted` |

Default Pantry aggregates (Fridge, Freezer, Dry goods) are created from `HouseholdCreated`, not by a command.

## Commands — stock

| # | Use case | Input | Rules | Event |
|---|---|---|---|---|
| S1 | **Restock** (`/stock:restock`) | `lines[]`: `ingredientId`, `amount`, `unit`, `expiry?` — **no pantry**: items are created to put away | FV2, FV3, FV4, FV6, FV8; all lines in one transaction | `ItemsRestocked`; may raise `StockOverstocked` / end `StockRunningLow` |
| S2 | **Put away / move item** (`/stock-items/{id}:move`) | `pantryId` (required), `amount?` + `unit?` (absent: whole item), `expiry?` | FV2, FV8, FV9, FV15 | `StockItemMoved`; `ShelfLifeRemembered` when learned |
| S3 | **Consume ingredient** (`/stock/{ingredientId}:consume`) | `amount`, `unit`, `pantryId?` | FV5, FV7; returns consumed items and shortfall | `StockItemConsumed` per item touched; may raise `StockRunningLow` |
| S4 | **Consume item** (`/stock-items/{id}:consume`) | `amount`, `unit?` (default: entered unit) | FV5, FV6, FV7 | `StockItemConsumed` |
| S5 | **Discard item** (`:discard`) | `reason` (`expired` \| `spoiled` \| `other`) | — | `StockItemDiscarded` |
| S6 | **Adjust item** (`:adjust`) | `remaining` (entered unit) | FV5; correction after a stock check, may exceed the entered amount | `StockItemAdjusted` |
| S7 | **Set expiry** (`:set-expiry`) | `date` or none | FV8; source becomes `manual`; any time, any kind | `StockItemExpiryChanged` |
| S8 | **Set threshold** (`/stock/{ingredientId}:set-threshold`) | `low?`, `over?` (base unit) | FV10, FV12 | `StockRunningLow` / `StockOverstocked` on state change |
| S9 | **Override expiry kind** (`/stock/{ingredientId}:override-expiry-kind`) | `kind` or none (back to the Foodpedia kind) | — | `ExpiryKindOverridden` |
| S10 | **Define household conversion** (`:define-conversion`) | `unit`, `unitAmount`, `baseAmount` | FV11; re-normalizes items (FV6) | `HouseholdConversionDefined` |
| S11 | **Remove household conversion** (`:remove-conversion`) | `unit` | Re-normalizes (Foodpedia default or unnormalized) | `HouseholdConversionRemoved` |

The restock response returns each created item with its normalized quantity, proposed expiry and `expiryMissing`, and a **conversion proposal** for unnormalized lines. The put-away response returns the moved (or split) item with its recomputed expiry and `expiryUnusual` when a typed date is far outside the household's usual range. A `foodventory.unit-not-convertible` problem (S2–S4) carries the ingredient and unit so the client can define a household conversion (S10) and retry. Writes on an existing aggregate use `If-Match` (ETag of the IngredientStock or Pantry aggregate); restock, move and consume accept an `Idempotency-Key`.

## Published API for Planning & Suggestion

### Read model (in-process query)

`GetStockSummary(householdId, asOf, expiringWithinDays = 3)` → `StockSummary`:

| Field | Meaning |
|---|---|
| `stateHash` | Hash of the household's stock versions — the stock part of the suggestion cache key (ADR 0005 §6). |
| `ingredients[]` | One entry per ingredient held or with a threshold: |
| · `ingredientId`, `code`, `baseUnit`, `expiryKind` | `code` is what goes into the compact LLM line; `expiryKind` is the effective one. |
| · `total` | Normalized total in the base unit, **put away or not**. |
| · `earliestExpiry` | Earliest known expiry among items, or none. |
| · `expiringAmount` | Normalized amount expiring within the window (or already expired). Never `longLife`. |
| · `unknownExpiryAmount` | Normalized amount with missing expiry (`printed` / `estimated`): unknown, **never counted as expiring**. |
| · `unnormalized[]` | Items without conversion: `amount`, `unit`, `expiry` — flagged, never summed. |
| · `state` | `ok` \| `low` \| `over` \| `no-threshold`. |

Planning computes coverage and "uses expiring" from this in code; only its one-line candidate summaries reach the LLM. A second operation, `GetStock(householdId, ingredientIds)`, returns the same entries for a subset (recipe coverage checks). Both are published in-process ([ADR 0010](../../architecture/adr/0010-modular-monolith-persistence.md)); the same view is exposed over HTTP as Q3 so suggestions can be tried **through the API before any UI**.

### Events consumed

| Event | From | Effect | Phase |
|---|---|---|---|
| `HouseholdCreated` | Identity & Household | Create the default Pantry aggregates (Fridge, Freezer, Dry goods) | MVP |
| `BaseUnitChanged`, `ConversionDefined`, `ConversionRemoved` | Foodpedia context | Re-normalize affected items (FV6) | MVP |
| `IngredientExpiryKindChanged` | Foodpedia context | Update the copied kind; flags and warnings follow, dates untouched | MVP |
| `IngredientRetired` / `IngredientRestored` | Foodpedia context | Flag / unflag items | MVP |
| `MealCooked` | Planning | Consume each recipe ingredient as S3 (clamped, shortfall reported), once per meal (FV14) | *later* |
| `ReceiptScanned` | Receipt Scanning | Restock as S1 with `source = receipt`, once per receipt line (FV14); unknown products arrive unnormalized | *later* |

### Events published

`ItemsRestocked`, `StockItemMoved`, `StockItemConsumed`, `StockItemDiscarded`, `StockItemAdjusted`, `StockItemExpiryChanged` (cache invalidation), `StockItemExpiringSoon` (suggestions), `StockRunningLow` / `StockOverstocked` (shopping list).

## Out of scope for MVP (later)

- Receipt and product (barcode) scanning; AI conversion proposals.
- Reading the printed date from a photo (the generic AI service).
- Suggesting an expiry-kind override when the user often corrects a pre-filled date.
- Prices paid (Market).
- Sharing pantries across households.
- Notifications for expiring items; waste statistics.
- Storage factors in Foodpedia shelf life; household setting for the expiry window.
