# E02.S01.T02 — Domain, persistence, migration and seed for the expiry kind

## Goal

`IngredientAggregate` has a required `ExpiryKind` (F12), set at create and changed by `SetExpiryKind`, which raises `IngredientExpiryKindChanged` only when the kind changes and is refused on a retired ingredient (F10). It is persisted as a string column, migrated, and seeded for every seed ingredient.

## Read only

- `dotnet/AGENTS.md` "Domain", "Persistence"
- [Foodpedia domain model](../../../contexts/foodpedia/domain-model.md) "ExpiryKind (enum)", "Invariants"
- `dotnet/src/Cooquoi.Context.Foodpedia/Domain/Ingredients/IngredientAggregate.cs:19-73`, `NewIngredient.cs`, `Storage.cs`
- `dotnet/src/Cooquoi.Context.Foodpedia/Persistence/Configurations/IngredientConfiguration.cs:27-43`
- `dotnet/src/Cooquoi.Context.Foodpedia/Persistence/Seed/SeedFile.cs:4-20`, `FoodpediaSeeder.cs:20-53`, `foodpedia-seed.json` (12 ingredients)
- `dotnet/tests/Cooquoi.Context.Foodpedia.Tests/Domain/Ingredients/IngredientAggregateTests.cs:68-110`

## Reference

- Enum: copy `Storage.cs` → `Domain/Ingredients/ExpiryKind.cs` (`Printed, Estimated, LongLife`).
- Behaviour + event: copy `DefineConversion` / `Retire` (`IngredientAggregate.cs:51-73`) and the 6-line event record `ConversionDefined.cs` → `IngredientExpiryKindChanged(IngredientId, ExpiryKind)`. Check F10 first, no-op when unchanged.
- EF: string conversion like `Status` (`IngredientConfiguration.cs:34`), column `expiry_kind`, kebab values (`long-life`) to match the contract.
- Migration `AddIngredientExpiryKind`: add the column with default `'estimated'` for existing rows, then drop the default (a hand-edit with a comment, per `dotnet/AGENTS.md` "Migrations").
- Seed: add `"expiryKind"` to each of the 12 ingredients, e.g. milk/yoghurt/meat `printed`, vegetables/bread `estimated`, rice/pasta `long-life`. Add it to `SeedFile.cs` and map it in `FoodpediaSeeder.ToIngredient` (`:53`).

## Skills

`dotnet-domain`, `dotnet-test`

## Model

sonnet

## Lookups

- Does `FoodpediaSeeder.SeedAsync` (`FoodpediaSeeder.cs:20-53`) update existing ingredients or only insert missing codes? Expected: ≤5 lines. If it only inserts, the migration default stands for existing dev rows; say so in the report.

## Out of scope

Request, DTO and endpoints (T03); PublicApi and integration events (T04); contract (T01)

## Subtasks

- [x] T02.1 `ExpiryKind`, `NewIngredient.ExpiryKind`, aggregate property and `SetExpiryKind`, `IngredientExpiryKindChanged`
- [x] T02.2 Domain tests: `F12_…` (create carries the kind), `C11_…` (changed raises once, same kind raises nothing), `F10_…` (retired refused)
- [x] T02.3 EF mapping and the `AddIngredientExpiryKind` migration
- [x] T02.4 Seed file, `SeedFile`, seeder mapping; the existing seeder test (if any) stays green

## Done when

`dotnet build dotnet/Cooquoi.slnx -v q -nologo -clp:ErrorsOnly && dotnet test --project dotnet/tests/Cooquoi.Context.Foodpedia.Tests -v q --no-progress` is green. `CreateIngredient` compiles because it passes a temporary `ExpiryKind.Estimated`, marked `// E02.S01.T03`, or T03 runs right after.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodpedia): expiry kind in the domain and seed (E02.S01.T02)`), stop.

Status: done

Report:
Added ExpiryKind, SetExpiryKind (F10, no-op when unchanged) and IngredientExpiryKindChanged; EF column `expiry_kind` (string like Status, stored PascalCase e.g. `LongLife`, not kebab), migration AddIngredientExpiryKind (default `Estimated` then dropped), seed + SeedFile + seeder mapping (accepts kebab).
Seeder only inserts missing codes, so existing dev rows keep the migration default `Estimated`.
Done-when green (81 tests, dotnet format clean); CreateIngredient passes `ExpiryKind.Estimated // E02.S01.T03`. T03 must map kebab to the enum.
