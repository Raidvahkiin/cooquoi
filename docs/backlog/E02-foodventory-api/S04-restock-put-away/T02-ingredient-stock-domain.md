# E02.S04.T02 — IngredientStock aggregate: restock, move and split, normalization, expiry proposal

## Goal

The pure domain of the IngredientStock aggregate covers restock and move/split:
- Types: `IngredientStockAggregate`, `StockItemEntity`, `Quantity`, `Expiry` (date + source), the Foodventory context's `ExpiryKind`.
- Domain services: normalization (`StockNormalizer`, FV6) and expiry proposal (`ExpiryProposer`, FV8).
- Events: `ItemsRestocked`, `StockItemMoved`.
- Rules FV3–FV9, each with domain tests named after its id.

## Read only

- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) lines 18–113 ("Aggregate boundaries" to "Expiry"), "Invariants" FV3–FV9, "Domain services", "Domain events"
- `dotnet/AGENTS.md` "Domain", "Hard rules" (`Domain/` uses only `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`)
- `dotnet/src/Cooquoi.SharedKernel/Unit.cs:36-44`
- `dotnet/src/Cooquoi.Context.Foodpedia/Domain/Ingredients/IngredientAggregate.cs` (aggregate shape), `ConversionTable.cs` (rule-carrying collection as a value object)
- `dotnet/src/Cooquoi.Context.Foodventory/Domain/Pantries/` (ids, `Storage`, from E02.S02.T04)
- `dotnet/tests/Cooquoi.Context.Foodpedia.Tests/Domain/Ingredients/IngredientAggregateTests.cs` (test naming)

## Reference

No aggregate with a child entity exists yet. Design inside these constraints:
- **No services in the aggregate.** Foodpedia data comes in as values that the use case resolves first. Example: a `FoodpediaFacts` record per ingredient (active, base unit, expiry kind, recommended storage, shelf-life days, default conversions per unit). Pass it to `Create` / `Restock` / `MoveItem`. `StockNormalizer` and `ExpiryProposer` are pure domain services over those values plus the aggregate's own state (household conversions arrive empty until E02.S06).
- `Restock(IReadOnlyList<NewStockLine>, FoodpediaFacts, DateOnly today)`: FV3 (active, else `StockError.IngredientRetired`; items have `PantryId = null`), FV4 (`Quantity` amount > 0, ≤ 3 decimals; known unit via `Unit`), FV6, FV8, then `ItemsRestocked`.
- `MoveItem(StockItemId, PantryTarget(PantryId, Storage?), Quantity? amount, DateOnly? expiry, FoodpediaFacts)`:
  - FV9: whole move, or split when `amount` < remaining. The amount converts to the entered unit, else `StockError.UnitNotConvertible(ingredientId, unit)`. Above remaining → `StockError.QuantityExceedsRemaining`.
  - FV8: recompute a proposed expiry for the target storage.
  - Raise `StockItemMoved`.
  - Leave a `// FV15: E02.S06.T02` seam where a typed date at put-away will be learned.
- `remaining` stays in the entered unit and `normalized` is derived (FV6). `Total` is the sum of normalized amounts (FV12 uses it later).
- Errors `StockError` (`foodventory.*` codes from T01). Ids `IngredientStockId`, `StockItemId` (UUID v7).
- ≤ 200 lines per aggregate file. Move normalization and proposal into the domain services so the aggregate stays under the limit.

## Skills

`dotnet-domain`, `dotnet-test`

## Model

opus (new aggregate with a child entity, cross-context inputs as values, normalization and split rules)

## Out of scope

EF mapping (T03); endpoints (T04, T05); consume and corrections (E02.S05); thresholds, household conversions, override, FV15 learning (E02.S06)

## Subtasks

- [ ] T02.1 Ids, `Quantity`, `Expiry`, `ExpiryKind`, `FoodpediaFacts`, `NewStockLine`
- [ ] T02.2 `StockNormalizer` + tests `FV6_…` (universal, Foodpedia default, unnormalized, never guessed)
- [ ] T02.3 `ExpiryProposer` + tests `FV8_…` (printed, estimated with storage none / matching / other, long-life, manual kept, past date allowed)
- [ ] T02.4 `IngredientStockAggregate.Create/Restock`, `StockItemEntity`, `ItemsRestocked` + tests `FV3_…`, `FV4_…`
- [ ] T02.5 `MoveItem` with split, `StockItemMoved` + tests `FV9_…` (whole, split keeps source/addedOn, equal = whole, exceeds, unit not convertible, exact entered unit always works)

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress --filter-namespace '*Domain*'` is green, or the whole project if the filter flag differs (`--help`). `dotnet test --project dotnet/tests/Cooquoi.Architecture.Tests -v q --no-progress` is green except catalogue lines for codes T01 has not merged yet.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): IngredientStock aggregate, restock and move (E02.S04.T02)`), stop.

Status: todo

Report:
