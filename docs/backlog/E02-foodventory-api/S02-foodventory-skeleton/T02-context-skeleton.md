# E02.S02.T02 — Foodventory context skeleton and architecture rules

## Goal

`dotnet/src/Cooquoi.Context.Foodventory` and `dotnet/tests/Cooquoi.Context.Foodventory.Tests` exist, are in `Cooquoi.slnx`, and are registered in the API host (context, Wolverine discovery, development migrate). `FoodventoryDbContext` uses schema `foodventory` and has an empty initial migration. The architecture tests know the new context and allow only `Integration/` → another context's `PublicApi/`.

## Read only

- `dotnet/AGENTS.md` "Layout", "Hard rules", "Persistence"
- `dotnet/src/Cooquoi.Context.Foodpedia/FoodpediaModule.cs:20-48` and its `.csproj`, `Persistence/FoodpediaDbContext.cs` (+ design-time factory)
- `dotnet/src/Cooquoi.Api/Program.cs:31-57`, `DevelopmentDatabase.cs:14-16`
- `dotnet/Cooquoi.slnx:4-5,17-18`
- `dotnet/tests/Cooquoi.Architecture.Tests/CooquoiArchitecture.cs:17-25`, `DependencyRulesTests.cs:31-39`
- `dotnet/tests/Cooquoi.Context.Foodpedia.Tests/Support/ApiFixture.cs:30-36` and its `.csproj`

## Reference

- Copy the Foodpedia project shape one to one: `FoodventoryModule.AddFoodventoryContext()` (Wolverine-integrated DbContext, `UseSchemaNpgsql(…, FoodventoryDbContext.Schema)`, `AddCooquoiInterceptors` with an empty `UniqueConstraints`), `MigrateAsync` for development (no seed), and `OnModelCreating` with `HasDefaultSchema`, `ApplyConfigurationsFromAssembly`, `AddAggregateAudit()`, `MapIdempotencyCommits()`.
- Initial migration `InitialFoodventory` (empty model; later tasks regenerate it, see `dotnet/AGENTS.md` "Persistence"; `dotnet tool restore` first; command in `dotnet/AGENTS.md` "Persistence").
- `Integration/CurrentHousehold`: a scoped service that resolves the caller's household id once per request through `Household.PublicApi` (from E02.S02.T01) using `CurrentUser` (`Platform/Authentication/CurrentUser.cs:13-18`). With no household it throws `CommonError.NotFound` (open question Q2). If T01 is not done yet, write against the interface named in its brief and finish the wiring in T03.
- Architecture: add the assembly to `OwnAssemblies`. Add pairwise rules Foodventory ↔ Foodpedia and Foodventory ↔ Household. Replace "contexts never reference each other" with "only `<Ctx>.Integration` may reference `<Other>.PublicApi`", and update the line in `dotnet/AGENTS.md` "Hard rules".
- The AppHost needs no change (one database, schema per context: `aspire/Cooquoi.AppHost/Resources/Database.cs:7-12`).

## Skills

`dotnet-feature` (module, migration), `dotnet-test` (architecture)

## Model

opus (first cross-context reference: the architecture rule change and the current-household seam)

## Out of scope

Pantry aggregate and handlers (T03); Household changes (T01)

## Subtasks

- [x] T02.1 Projects, csproj references (Household and Foodpedia only for `Integration/`), slnx
- [x] T02.2 `FoodventoryDbContext`, design-time factory, module, initial migration
- [x] T02.3 `Program.cs` (`AddFoodventoryContext`, `Discovery.IncludeAssembly`), `DevelopmentDatabase`
- [x] T02.4 `Integration/CurrentHousehold`
- [x] T02.5 Architecture tests: assemblies, pair rules, the `Integration/` → `PublicApi/` rule; `dotnet/AGENTS.md` line
- [x] T02.6 Test project with `Support/ApiFixture` (copy Foodpedia's) and one smoke test (the schema exists after startup)

## Done when

`dotnet build dotnet/Cooquoi.slnx -v q -nologo -clp:ErrorsOnly && dotnet test --project dotnet/tests/Cooquoi.Architecture.Tests -v q --no-progress && dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress && dotnet format dotnet/Cooquoi.slnx --verify-no-changes` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): context skeleton (E02.S02.T02)`), stop.

Status: done

Report:
