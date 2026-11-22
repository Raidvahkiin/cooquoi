# E02.S04.T03 — Persistence and the Foodpedia adapter

## Goal

IngredientStock aggregates and their stock items are stored in `foodventory.ingredient_stocks` and `foodventory.stock_items` with the read-side indexes and optimistic concurrency. `Integration/FoodpediaFactsProvider` builds the domain's `FoodpediaFacts` from the Foodpedia published API (P1–P3).

## Read only

- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "Persistence notes"
- `dotnet/AGENTS.md` "Persistence" (mapping table, version, uniqueness, migrations)
- `dotnet/src/Cooquoi.Context.Foodpedia/Persistence/Configurations/IngredientConfiguration.cs:27-43` (owned types, jsonb, enum strings, converters)
- `dotnet/src/Cooquoi.Context.Foodventory/Domain/IngredientStocks/` (from T02)
- `dotnet/src/Cooquoi.Context.Foodpedia/PublicApi/` (from E02.S01.T04)

## Reference

- `IngredientStockConfiguration`: version `IsConcurrencyToken()`, `Ignore(DomainEvents)`, unique `ux_ingredient_stocks_household_ingredient` → `StockError` conflict, or let the T04 endpoint retry (see T04). `base_unit`, `expiry_kind` columns. Columns for E02.S06 (threshold, conversions jsonb, override, remembered shelf lives) are added there, not now.
- `stock_items`: `HasMany(Items)` to its own table (an entity with an id: `OwnsMany` with key `id`, or a mapped entity; pick what keeps `AggregateSaveInterceptor` bumping the root version on child changes, see `dotnet/AGENTS.md` "Responses and HTTP semantics" Concurrency). Columns: `household_id` (copied, for indexes), `ingredient_id` (copied), nullable `pantry_id`, entered amount/unit, remaining amount, `normalized_amount` (nullable), `expires_on`, `expiry_source`, `added_on`, `source`, `source_ref`. Indexes: `(household_id, expires_on)`, `(household_id, pantry_id)`.
- Migration `AddIngredientStocks`.
- Adapter: `Integration/FoodpediaFactsProvider.GetAsync(IReadOnlyCollection<Guid> ingredientIds, Locale)` → `IReadOnlyDictionary<IngredientId, FoodpediaFacts>`. It makes one P1 call; Foodpedia default conversions come from P1 if S01.T04 exposed them, else P2 per unit. It is the only file that references Foodpedia (`Integration/` rule from E02.S02.T02).

## Skills

`dotnet-feature` (persistence part), `dotnet-test` (persistence tests)

## Model

sonnet

## Out of scope

Endpoints (T04, T05); E02.S06 columns

## Subtasks

- [ ] T03.1 `IngredientStockConfiguration`, stock items mapping, converters
- [ ] T03.2 Migration `AddIngredientStocks` (check the SQL has both indexes and the unique index)
- [ ] T03.3 `FoodpediaFactsProvider` + registration
- [ ] T03.4 Persistence tests: round-trip with items in and out of a pantry; version bumps when only a child changes; adapter returns facts for a seeded ingredient

## Done when

`dotnet build dotnet/Cooquoi.slnx -v q -nologo -clp:ErrorsOnly && dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress && dotnet test --project dotnet/tests/Cooquoi.Architecture.Tests -v q --no-progress` is green (catalogue lines aside, as in T02).

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): ingredient stock persistence and Foodpedia adapter (E02.S04.T03)`), stop.

Status: todo

Report:
