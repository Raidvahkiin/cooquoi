# Foodpedia — Use cases

- **Status**: Reviewed — ready for the spike
- Domain model: [domain-model.md](domain-model.md). Contract style: uniform reads, named actions for writes ([ADR 0009](../../architecture/adr/0009-api-contract-typespec.md)).

## Actors

| Actor | Can |
|---|---|
| **User** (any signed-in user) | Search and read ingredients and tags, list units |
| **Curator** (`catalog-curator` role) | Everything a user can, plus manage ingredients and tags. Changes apply immediately. |
| **Other contexts** (Pantry, Cookbook, Market, Planning) | Use the published in-process API |

## Queries

| # | Use case | Input | Output | Notes |
|---|---|---|---|---|
| Q1 | **Search ingredients** | `q`, `tags?` (all must match), `status?` (curators only), `sort`, `page`, `pageSize` | `Page<IngredientSummary>` (id, code, name in requested locale, base unit, category tags) | Matches primary names and synonyms in **all locales**, active locale ranked first, accent-insensitive. Retired hidden for users. |
| Q2 | **Get ingredient** | `ingredientId` | `IngredientDetail` (name and synonyms in requested locale, base unit, conversions, shelf life, expiry kind, tags, status) | Locale from `Accept-Language`, fallback English. |
| Q3 | **Get ingredient translations** *(curator)* | `ingredientId` | All `LocalizedName`s, missing locales flagged | For editing. |
| Q4 | **List missing translations** *(curator)* | `locale`, `page` | `Page<IngredientSummary>` | Helps complete `fr` / `vi`. |
| Q5 | **List tags** | `namespace?`, `page` | `Page<TagSummary>` (code, label in requested locale) | For filters and editing. |
| Q6 | **List units** | — | Unit catalogue (code, dimension) | Labels translated by the client. |
| Q7 | **Get change history** *(curator)* | `ingredientId`, `page` | Events with author and time | From domain events. |

## Commands — ingredients (curator)

| # | Use case | Input | Rules | Event |
|---|---|---|---|---|
| C1 | **Create ingredient** | `code`, `names` (English required), `baseUnit`, `expiryKind`, `tags` (≥ 1 `category:`), `shelfLife?`, `conversions?` | F1–F9, F12 | `IngredientCreated` |
| C2 | **Rename** (`:rename`) | `locale`, `primary` | F3, F4 | `IngredientNamesChanged` |
| C3 | **Set synonyms** (`:set-synonyms`) | `locale`, `synonyms[]` | — | `IngredientNamesChanged` |
| C4 | **Change base unit** (`:change-base-unit`) | `baseUnit`, conversion between old and new base unit | F5 | `BaseUnitChanged` |
| C5 | **Define conversion** (`:define-conversion`) | `unit`, `unitAmount`, `baseAmount` | F6, F7; replaces an existing one for the same unit | `ConversionDefined` |
| C6 | **Remove conversion** (`:remove-conversion`) | `unit` | — | `ConversionRemoved` |
| C7 | **Set shelf life** (`:set-shelf-life`) | `days` + `recommendedStorage`, or none | F8 | `ShelfLifeChanged` |
| C8 | **Tag / untag** (`:tag`, `:untag`) | `tagCode` | F9 | `IngredientTagsChanged` |
| C9 | **Retire** (`:retire`) | — | F10 | `IngredientRetired` |
| C10 | **Restore** (`:restore`) | — | — | `IngredientRestored` |
| C11 | **Set expiry kind** (`:set-expiry-kind`) | `printed` \| `estimated` \| `long-life` | F12 | `IngredientExpiryKindChanged` |

## Commands — tags (curator)

| # | Use case | Input | Rules | Event |
|---|---|---|---|---|
| T1 | **Create tag** | `code` (`<namespace>:<value>`), `labels` (English required) | F1, F2 | `TagCreated` |
| T2 | **Set label** (`:set-label`) | `locale`, `label` | F3 | `TagLabelsChanged` |
| T3 | **Retire tag** (`:retire`) | — | Not allowed if it is the only `category:` tag of an active ingredient (F9) | `TagRetired` |

All commands: F10 (no change on retired items except restore) and F11 (curator only). Errors return `ProblemDetails` with stable codes, e.g. `foodpedia.code-taken`, `foodpedia.name-taken`, `foodpedia.conversion-not-needed`, `foodpedia.base-unit-conversion-required`, `foodpedia.category-tag-required`, `foodpedia.ingredient-retired`.

## Published API for other contexts (in-process)

| # | Operation | Used by | Behaviour |
|---|---|---|---|
| P1 | `GetIngredients(ids, locale)` | Pantry, Cookbook, Market, Planning | Batch read for display and validation (status, base unit, expiry kind, tags). |
| P2 | `NormalizeQuantity(ingredientId, amount, unit)` | Pantry, Cookbook, Market | Amount in the base unit using Foodpedia defaults, or *unnormalized*. Pantry applies household conversions first. |
| P3 | `ProposeShelfLife(ingredientId, from)` | Pantry | Proposed expiry date and recommended storage, or none. |

## Seed

1. **Import** (one shot, script): fetch candidate ingredients with `en` / `fr` / `vi` names from Wikidata (CC0).
2. **Map** to our model: generate `code`s, primary names, synonyms.
3. **Complete** with AI proposals: base unit, conversions, shelf life + storage, expiry kind, tags.
4. **Review** by the curator, then commit the seed files (keyed by `code`).
5. **Load**: idempotent seeding at startup in development, once per release in production.

Target ≈200–300 common ingredients. The script is kept in the repo for later batches but is never a runtime dependency.

## Out of scope for MVP

- Household-private ingredients and promotion to Foodpedia.
- Region-specific seasons (months per region).
- Nutrition data.
- Draft/publish workflow for curators.
