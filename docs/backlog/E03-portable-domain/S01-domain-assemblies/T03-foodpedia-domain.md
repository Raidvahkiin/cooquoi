# E03.S01.T03 — Foodpedia domain assembly and domain test project, caller-supplied `TagId`

## Goal

`Cooquoi.Context.Foodpedia.Domain` holds everything that was `Cooquoi.Context.Foodpedia/Domain/`, with the same namespaces, and is AOT-compatible. Its unit tests move to `Cooquoi.Context.Foodpedia.Domain.Tests`. `TagAggregate.Create` takes its id from the caller. No API behaviour changes.

## Read only

- `docs/architecture/adr/0023-domain-only-assemblies.md` Decision
- `dotnet/AGENTS.md` "Domain"
- `dotnet/src/Cooquoi.Context.Foodventory.Domain/Cooquoi.Context.Foodventory.Domain.csproj` (from T02): copy it
- `dotnet/src/Cooquoi.Context.Foodpedia/Cooquoi.Context.Foodpedia.csproj`
- `dotnet/tests/Cooquoi.Architecture.Tests/CooquoiArchitecture.cs:17-28`

## Reference

- Do the same as T02 (its commit is the pattern): a new project `dotnet/src/Cooquoi.Context.Foodpedia.Domain/`, `git mv` of every folder under `Cooquoi.Context.Foodpedia/Domain/`, a `ProjectReference` from the context project, a `Cooquoi.slnx` entry next to line 4, and the assembly added to `OwnAssemblies`.
- **Domain test project:** same steps as T02, in `dotnet/tests/Cooquoi.Context.Foodpedia.Domain.Tests/`, referencing only the domain project. Add it to `Cooquoi.slnx` and `git mv` `tests/Cooquoi.Context.Foodpedia.Tests/Domain/*` into it. What's different here:
  - `Support/DomainAssertions.cs`: only used by domain tests (checked). Move it to the domain test project's `Support/`.
  - `Support/SampleIngredients.cs`: also used by 4 integration tests (`PublicApi/FoodpediaCatalogTests.cs`, `Persistence/ConstraintViolationTests.cs`, `Persistence/Configurations/ConversionTableJsonTests.cs`, `Persistence/AggregateVersionTests.cs`). Move it to the domain test project and link it back into the context test project with `<Compile Include=… Link=…/>`, as T02 did for `TestFacts`.
  - `Domain/Ingredients/ContractEnumNamesTests.cs`: this tests `Cooquoi.Platform.OpenApi.ContractEnumNames`, not the domain, so it can't go into a project that references only the domain. Move it to `tests/Cooquoi.Context.Foodpedia.Tests/OpenApi/ContractEnumNamesTests.cs` in the context test project.
- **Ids:** `Domain/Tags/TagAggregate.cs:26` calls `TagId.New()`. Add a `TagId id` parameter (or put it into the `NewTag` input record if one exists), and let the callers pass `TagId.New()`. The callers are the tag endpoint or handler and the seed under `Persistence/Seed/`.
- **Regex:** the domain uses `System.Text.RegularExpressions` in 2 files. It is AOT-safe. If the analyzers suggest `[GeneratedRegex]` (SYSLIB1045), apply it, since it is a source generator and also faster.
- **Cross-context:** `Cooquoi.Context.Foodventory` references `Cooquoi.Context.Foodpedia` for its `PublicApi`. Check that nothing in `Foodpedia/PublicApi` exposes a domain type. If something does, the reference still works through the context project; just report it.

## Skills

`dotnet-domain`, `dotnet-test`, `refactor`

## Model

sonnet

## Out of scope

- Household and the architecture tests (T04).
- Contract changes.

## Subtasks

- [ ] T03.1 Project, `git mv`, references, `Cooquoi.slnx`, `OwnAssemblies`.
- [ ] T03.2 `TagId` from the caller; update the callers and the tests.
- [ ] T03.3 Domain test project: `git mv` the tests, move `DomainAssertions`, link `SampleIngredients`, move `ContractEnumNamesTests`. `ls dotnet/tests/Cooquoi.Context.Foodpedia.Tests/Domain` fails.
- [ ] T03.4 `grep -rn '\.New()\|Guid.NewGuid\|DateTime\.\|TimeProvider' dotnet/src/Cooquoi.Context.Foodpedia.Domain` prints nothing.

## Done when

Same as T02:
- `dotnet build dotnet/Cooquoi.slnx -v q` has 0 warnings;
- `dotnet format dotnet/Cooquoi.slnx --verify-no-changes` passes;
- the full `dotnet test` is green, with at least as many tests as before;
- `pnpm nx run-many -t drift` reports `none`.

Then:
- tick the subtasks;
- write the report;
- set the status;
- commit `refactor(foodpedia): domain-only assembly, domain tests and caller-supplied tag id (E03.S01.T03)`;
- stop.

Status: todo

Report:
