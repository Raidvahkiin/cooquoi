# Pantry — Use cases

- **Status**: Draft — see open questions in the domain model
- Domain model: [domain-model.md](domain-model.md). Contract style: uniform reads, named actions for writes ([ADR 0009](../../architecture/adr/0009-api-contract-typespec.md)). API only; no UI in this phase.
- Every operation is scoped to the caller's household (P2). **MVP** unless marked *later*.

## Actors

| Actor | Can |
|---|---|
| **Household member** (any signed-in user, in MVP the sole member) | Everything below for their own household |
| **Planning & Suggestion** | Read the published read model; publish `MealCooked` (*later*) |
| **Receipt Scanning** *(later)* | Publish `ItemsRestocked` |
| **Foodpedia** | Publish ingredient changes that trigger re-normalization |

## Queries

| # | Use case | Input | Output | Notes |
|---|---|---|---|---|
| Q1 | **List pantries** | — | `PantrySummary[]` (id, name, storage, item count) | Small; not paged. |
| Q2 | **Stock by pantry** | `pantryId`, `tags?`, `sort` (`expiry` \| `name` \| `added`), `page` | `Page<StockItemView>` (item id, ingredient id + name in locale, entered, remaining, normalized + base unit or *unnormalized*, expiry + source, added on) | J2 "what I have". |
| Q3 | **Stock by ingredient** | `q?`, `tags?`, `pantryId?`, `page` | `Page<IngredientStockView>` (ingredient, base unit, **total**, unnormalized items, earliest expiry, low/over state) | Total summed in the base unit across pantries (P6). |
| Q4 | **Get ingredient stock** | `ingredientId` | Items per pantry, total, threshold, household conversions | Detail and editing. |
| Q5 | **Expiring soon** | `withinDays` (default 3), `includeExpired` (default true), `page` | `Page<StockItemView>` ordered by expiry | J2 "what expires soon". |
| Q6 | **Low stock** | `includeOverstock?`, `page` | `Page<IngredientStockView>` in low (or over) state | Only ingredients with a threshold. |

## Commands — pantries

| # | Use case | Input | Rules | Event |
|---|---|---|---|---|
| C1 | **Create pantry** | `name`, `storage?` | P1 | `PantryCreated` |
| C2 | **Rename** (`:rename`) | `name` | P1 | `PantryRenamed` |
| C3 | **Set storage** (`:set-storage`) | `storage` or none | — | `PantryStorageChanged` |
| C4 | **Delete pantry** | — | P12 | `PantryDeleted` |

## Commands — stock

| # | Use case | Input | Rules | Event |
|---|---|---|---|---|
| S1 | **Restock** (`/stock:restock`) | `lines[]`: `ingredientId`, `amount`, `unit`, `pantryId?`, `expiry?` | P2–P4, P6, P8; all lines in one transaction | `StockItemAdded` per line; may raise `StockOverstocked` / end `StockRunningLow` |
| S2 | **Consume ingredient** (`/stock/{ingredientId}:consume`) | `amount`, `unit`, `pantryId?` | P5, P7; returns consumed items and shortfall | `StockItemConsumed` per item touched; may raise `StockRunningLow` |
| S3 | **Consume item** (`/stock-items/{id}:consume`) | `amount`, `unit?` (default: entered unit) | P5, P6 | `StockItemConsumed` |
| S4 | **Discard item** (`:discard`) | `reason` (`expired` \| `spoiled` \| `other`) | — | `StockItemDiscarded` |
| S5 | **Adjust item** (`:adjust`) | `remaining` (entered unit) | P5; correction after a stock check, may exceed the entered amount | `StockItemAdjusted` |
| S6 | **Set expiry** (`:set-expiry`) | `date` or none | P8; source becomes `manual` | `StockItemExpiryChanged` |
| S7 | **Move item** (`:move`) | `pantryId` | P2; whole item | `StockItemMoved` |
| S8 | **Set threshold** (`/stock/{ingredientId}:set-threshold`) | `low?`, `over?` (base unit) | P9, P11 | `StockRunningLow` / `StockOverstocked` on state change |
| S9 | **Define household conversion** (`:define-conversion`) | `unit`, `unitAmount`, `baseAmount` | P10; re-normalizes items (P6) | `HouseholdConversionDefined` |
| S10 | **Remove household conversion** (`:remove-conversion`) | `unit` | Re-normalizes (Foodpedia default or unnormalized) | `HouseholdConversionRemoved` |

The restock response returns each created item with its normalized quantity, proposed expiry and pantry, and a **conversion proposal** for unnormalized lines. Writes on an existing aggregate use `If-Match` (ETag of the `IngredientStock` or `Pantry`); restock and consume accept an `Idempotency-Key`.

## Published API for Planning & Suggestion

### Read model (in-process query)

`GetStockSummary(householdId, asOf, expiringWithinDays = 3)` → `StockSummary`:

| Field | Meaning |
|---|---|
| `stateHash` | Hash of the household's stock versions — the pantry part of the suggestion cache key (ADR 0005 §6). |
| `ingredients[]` | One entry per ingredient held or with a threshold: |
| · `ingredientId`, `code`, `baseUnit` | `code` is what goes into the compact LLM line. |
| · `total` | Normalized total in the base unit, all pantries. |
| · `earliestExpiry` | Earliest expiry among items, or none. |
| · `expiringAmount` | Normalized amount expiring within the window (or already expired). |
| · `unnormalized[]` | Items without conversion: `amount`, `unit`, `expiry` — flagged, never summed. |
| · `state` | `ok` \| `low` \| `over` \| `no-threshold`. |

Planning computes coverage and "uses expiring" from this in code; only its one-line candidate summaries reach the LLM. A second operation, `GetStock(householdId, ingredientIds)`, returns the same entries for a subset (recipe coverage checks). Both are published in-process ([ADR 0010](../../architecture/adr/0010-modular-monolith-persistence.md)); the same view is exposed over HTTP as Q3 so suggestions can be tried **through the API before any UI**.

### Events consumed

| Event | From | Effect | Phase |
|---|---|---|---|
| `HouseholdCreated` | Identity & Household | Create the first pantry | MVP |
| `BaseUnitChanged`, `ConversionDefined`, `ConversionRemoved` | Foodpedia | Re-normalize affected items (P6) | MVP |
| `IngredientRetired` / `IngredientRestored` | Foodpedia | Flag / unflag items | MVP |
| `MealCooked` | Planning | Consume each recipe ingredient as S2 (clamped, shortfall reported), once per meal (P13) | *later* |
| `ItemsRestocked` | Receipt Scanning | Restock as S1 with `source = receipt`, once per receipt line (P13); unknown products arrive unnormalized | *later* |

### Events published

`StockItemAdded`, `StockItemConsumed`, `StockItemDiscarded`, `StockItemAdjusted`, `StockItemMoved`, `StockItemExpiryChanged` (cache invalidation), `StockItemExpiringSoon` (suggestions), `StockRunningLow` / `StockOverstocked` (shopping list).

## Out of scope for MVP

- Receipt and product (barcode) scanning; AI conversion proposals.
- Prices paid (Market).
- Splitting an item on move; sharing pantries across households.
- Notifications for expiring items; waste statistics.
- Storage-dependent shelf life (freezer factors).
