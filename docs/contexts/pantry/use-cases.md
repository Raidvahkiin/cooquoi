# Pantry — Use cases

- **Status**: Reviewed — decisions of 2026-11-22 applied
- Domain model: [domain-model.md](domain-model.md). Contract style: uniform reads, named actions for writes ([ADR 0009](../../architecture/adr/0009-api-contract-typespec.md)). API only; no UI in this phase.
- Every operation is scoped to the caller's household (P2). **MVP** unless marked *later*.

## Actors

| Actor | Can |
|---|---|
| **Household member** (any signed-in user, in MVP the sole member) | Everything below for their own household |
| **Planning & Suggestion** | Read the published read model; publish `MealCooked` (*later*) |
| **Receipt Scanning** *(later)* | Publish `ReceiptScanned` |
| **Foodpedia** | Publish ingredient changes that trigger re-normalization |

## Queries

| # | Use case | Input | Output | Notes |
|---|---|---|---|---|
| Q1 | **List pantries** | — | `PantrySummary[]` (id, kind, name, storage, item count) | Inbox first; small, not paged. |
| Q2 | **Stock by pantry** | `pantryId` or `inbox=true`, `tags?`, `sort` (`expiry` \| `name` \| `added`), `page` | `Page<StockItemView>` (item id, ingredient id + name in locale, entered, remaining, normalized + base unit or *unnormalized*, expiry + source, added on; for inbox items a **suggested pantry**) | J2 "what I have"; `inbox=true` is "what's still to put away". |
| Q3 | **Stock by ingredient** | `q?`, `tags?`, `pantryId?`, `page` | `Page<IngredientStockView>` (ingredient, base unit, **total**, unnormalized items, earliest expiry, low/over state) | Total summed in the base unit across pantries, inbox included (P7, P13). |
| Q4 | **Get ingredient stock** | `ingredientId` | Items per pantry, total, threshold, household conversions | Detail and editing. |
| Q5 | **Expiring soon** | `withinDays` (default 3), `includeExpired` (default true), `page` | `Page<StockItemView>` ordered by expiry | J2 "what expires soon"; inbox included. |
| Q6 | **Low stock** | `includeOverstock?`, `page` | `Page<IngredientStockView>` in low (or over) state | Only ingredients with a threshold. |

## Commands — pantries

| # | Use case | Input | Rules | Event |
|---|---|---|---|---|
| C1 | **Create pantry** | `name`, `storage?` | P1; always `regular` | `PantryCreated` |
| C2 | **Rename** (`:rename`) | `name` | P1, P3 (not the inbox) | `PantryRenamed` |
| C3 | **Set storage** (`:set-storage`) | `storage` or none | P3 (not the inbox) | `PantryStorageChanged` |
| C4 | **Delete pantry** | — | P3, P14 | `PantryDeleted` |

Default pantries (To put away, Fridge, Freezer, Cupboard) are created from `HouseholdCreated`, not by a command.

## Commands — stock

| # | Use case | Input | Rules | Event |
|---|---|---|---|---|
| S1 | **Restock** (`/stock:restock`) | `lines[]`: `ingredientId`, `amount`, `unit`, `expiry?` — **no pantry**: always the inbox | P2, P4, P5, P7, P9; all lines in one transaction | `ItemsRestocked`; may raise `StockOverstocked` / end `StockRunningLow` |
| S2 | **Put away / move item** (`/stock-items/{id}:move`) | `pantryId` (regular), `amount?` + `unit?` (absent: whole item) | P2, P3, P9, P10 | `StockItemMoved` |
| S3 | **Consume ingredient** (`/stock/{ingredientId}:consume`) | `amount`, `unit`, `pantryId?` | P6, P8; returns consumed items and shortfall | `StockItemConsumed` per item touched; may raise `StockRunningLow` |
| S4 | **Consume item** (`/stock-items/{id}:consume`) | `amount`, `unit?` (default: entered unit) | P6, P7, P8 | `StockItemConsumed` |
| S5 | **Discard item** (`:discard`) | `reason` (`expired` \| `spoiled` \| `other`) | — | `StockItemDiscarded` |
| S6 | **Adjust item** (`:adjust`) | `remaining` (entered unit) | P6; correction after a stock check, may exceed the entered amount | `StockItemAdjusted` |
| S7 | **Set expiry** (`:set-expiry`) | `date` or none | P9; source becomes `manual` | `StockItemExpiryChanged` |
| S8 | **Set threshold** (`/stock/{ingredientId}:set-threshold`) | `low?`, `over?` (base unit) | P11, P13 | `StockRunningLow` / `StockOverstocked` on state change |
| S9 | **Define household conversion** (`:define-conversion`) | `unit`, `unitAmount`, `baseAmount` | P12; re-normalizes items (P7) | `HouseholdConversionDefined` |
| S10 | **Remove household conversion** (`:remove-conversion`) | `unit` | Re-normalizes (Foodpedia default or unnormalized) | `HouseholdConversionRemoved` |

The restock response returns each created item with its normalized quantity and proposed expiry, and a **conversion proposal** for unnormalized lines. A `pantry.unit-not-convertible` problem (S2–S4) carries the ingredient and unit so the client can define a household conversion (S9) and retry. Writes on an existing aggregate use `If-Match` (ETag of the `IngredientStock` or `Pantry`); restock, move and consume accept an `Idempotency-Key`.

## Published API for Planning & Suggestion

### Read model (in-process query)

`GetStockSummary(householdId, asOf, expiringWithinDays = 3)` → `StockSummary`:

| Field | Meaning |
|---|---|
| `stateHash` | Hash of the household's stock versions — the pantry part of the suggestion cache key (ADR 0005 §6). |
| `ingredients[]` | One entry per ingredient held or with a threshold: |
| · `ingredientId`, `code`, `baseUnit` | `code` is what goes into the compact LLM line. |
| · `total` | Normalized total in the base unit, all pantries **including the inbox**. |
| · `earliestExpiry` | Earliest expiry among items, or none. |
| · `expiringAmount` | Normalized amount expiring within the window (or already expired). |
| · `unnormalized[]` | Items without conversion: `amount`, `unit`, `expiry` — flagged, never summed. |
| · `state` | `ok` \| `low` \| `over` \| `no-threshold`. |

Planning computes coverage and "uses expiring" from this in code; only its one-line candidate summaries reach the LLM. A second operation, `GetStock(householdId, ingredientIds)`, returns the same entries for a subset (recipe coverage checks). Both are published in-process ([ADR 0010](../../architecture/adr/0010-modular-monolith-persistence.md)); the same view is exposed over HTTP as Q3 so suggestions can be tried **through the API before any UI**.

### Events consumed

| Event | From | Effect | Phase |
|---|---|---|---|
| `HouseholdCreated` | Identity & Household | Create the default pantries (inbox, Fridge, Freezer, Cupboard) | MVP |
| `BaseUnitChanged`, `ConversionDefined`, `ConversionRemoved` | Foodpedia | Re-normalize affected items (P7) | MVP |
| `IngredientRetired` / `IngredientRestored` | Foodpedia | Flag / unflag items | MVP |
| `MealCooked` | Planning | Consume each recipe ingredient as S3 (clamped, shortfall reported), once per meal (P15) | *later* |
| `ReceiptScanned` | Receipt Scanning | Restock as S1 into the inbox with `source = receipt`, once per receipt line (P15); unknown products arrive unnormalized | *later* |

### Events published

`ItemsRestocked`, `StockItemMoved`, `StockItemConsumed`, `StockItemDiscarded`, `StockItemAdjusted`, `StockItemExpiryChanged` (cache invalidation), `StockItemExpiringSoon` (suggestions), `StockRunningLow` / `StockOverstocked` (shopping list).

## Out of scope for MVP

- Receipt and product (barcode) scanning; AI conversion proposals.
- Prices paid (Market).
- Sharing pantries across households.
- Notifications for expiring items; waste statistics.
- Storage-dependent shelf life (freezer factors); household setting for the expiry window.
