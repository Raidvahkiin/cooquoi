# E02.S01.T04 — Foodpedia published API (P1–P3) and integration events

## Goal

The Foodpedia context publishes in-process what the Foodventory context needs, without exposing `Domain/`:
- P1 `GetIngredients(ids, locale)`: status, base unit, expiry kind, recommended storage, code, name in locale.
- P2 `NormalizeQuantity(ingredientId, amount, unit)`: universal factor, else Foodpedia conversion, else unnormalized.
- P3 `ProposeShelfLife(ingredientId, from)`.
- Integration events for `IngredientExpiryKindChanged`, `ConversionDefined` and `IngredientRetired`.

This sets the `PublicApi/` convention that the Household context (E02.S02.T01) and the Foodventory context (E02.S08.T02) copy.

## Read only

- [ADR 0010](../../../architecture/adr/0010-modular-monolith-persistence.md) "Decision" §1
- `dotnet/AGENTS.md` "Layout" (`Integration/  PublicApi/`), "Hard rules"
- [Foodpedia use cases](../../../contexts/foodpedia/use-cases.md) "Published API for other contexts (in-process)"
- `dotnet/src/Cooquoi.Context.Foodpedia/Domain/Ingredients/ConversionTable.cs:19-23`, `Conversion.cs:7-20`, `ShelfLife.cs:6-17`
- `dotnet/src/Cooquoi.SharedKernel/Unit.cs:36-44` (`FactorToDimensionBase`, `ConvertsUniversallyTo`)
- `dotnet/src/Cooquoi.Context.Foodpedia/Persistence/Projections/IngredientSearchTermsHandler.cs:7-16` (handler shape)

## Reference

No `PublicApi/` exists yet. Design it:
- `PublicApi/IFoodpediaCatalog.cs` (or a sealed class registered in `AddFoodpediaContext`, `FoodpediaModule.cs:29-38`) with P1–P3, returning `PublicApi` records built from primitives and `Cooquoi.SharedKernel` types only (`Unit`, `Locale`).
- Reads project from `FoodpediaDbContext` with `AsNoTracking` (the query rule in `dotnet/AGENTS.md` "Use cases"). Domain logic stays in the domain: add `ConversionTable.Normalize(Quantity…)` (or similar) and call it from P2.
- Integration events: records in `PublicApi/Events/` (e.g. `IngredientExpiryKindChanged` with ingredient id and kind as a kebab string or a `PublicApi` enum), published by a `…Handler` in `Integration/` that maps the domain event (Wolverine cascading return). Recommended over moving domain events, so `Domain/` stays internal. Record the choice in the report and in `dotnet/AGENTS.md` "Layout" (one line).

## Skills

`dotnet-domain` (`Normalize`), `dotnet-feature`, `dotnet-test`

## Model

opus (first published API and integration-event convention, with no pattern to copy)

## Out of scope

Endpoints (T03); architecture-test rules for consumers (E02.S02.T02); Foodventory code

## Subtasks

- [ ] T04.1 Domain: normalization method on `ConversionTable` + domain tests (universal, conversion, unnormalized)
- [ ] T04.2 `PublicApi/` P1–P3 and records; registration in `FoodpediaModule`
- [ ] T04.3 Integration tests for P1–P3 against the seeded database (`Cooquoi.Testing` `TestApi.CreateDbContext`, `TestApi.cs:41`)
- [ ] T04.4 `PublicApi/Events/` records + mapping handlers; tracked-session tests that each domain event (sent with `InvokeMessageAndWaitAsync`) publishes its integration event, with no dependency on the T03 endpoint
- [ ] T04.5 One line in `dotnet/AGENTS.md` "Layout" on `PublicApi/Events/`

## Done when

`dotnet build dotnet/Cooquoi.slnx -v q -nologo -clp:ErrorsOnly && dotnet test --project dotnet/tests/Cooquoi.Context.Foodpedia.Tests -v q --no-progress && dotnet test --project dotnet/tests/Cooquoi.Architecture.Tests -v q --no-progress` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodpedia): published API and integration events (E02.S01.T04)`), stop.

Status: todo

Report:
