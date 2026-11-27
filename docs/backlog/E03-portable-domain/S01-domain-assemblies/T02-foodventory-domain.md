# E03.S01.T02 — Foodventory domain assembly and domain test project, caller-supplied ids, AOT flags on the building blocks

## Goal

`Cooquoi.Context.Foodventory.Domain` holds everything that was `Cooquoi.Context.Foodventory/Domain/`, with the same namespaces, and its unit tests move to `Cooquoi.Context.Foodventory.Domain.Tests`. It is AOT-compatible, as are `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`. `IngredientStockAggregate` and `DefaultPantries` no longer create ids: their callers pass them in. No API behaviour changes.

## Read only

- ADR 0023 Decision (written by T01): `docs/architecture/adr/0023-domain-only-assemblies.md`
- `dotnet/AGENTS.md` "Layout", "Domain", "Commands"
- `dotnet/src/Cooquoi.Context.Foodventory/Cooquoi.Context.Foodventory.csproj`: whole file, 13 lines
- `dotnet/src/Cooquoi.SharedKernel/Cooquoi.SharedKernel.csproj`, `dotnet/src/BuildingBlocks/Raidvahkiin.Ddd/Raidvahkiin.Ddd.csproj`: a few lines each
- `dotnet/tests/Cooquoi.Architecture.Tests/CooquoiArchitecture.cs:17-28`, `MirroredTestRulesTests.cs:51-57`
- `dotnet/tests/Directory.Build.props` (test packages every test project gets) and `dotnet/tests/Cooquoi.Context.Foodventory.Tests/Cooquoi.Context.Foodventory.Tests.csproj`

## Reference

- **New project** `dotnet/src/Cooquoi.Context.Foodventory.Domain/Cooquoi.Context.Foodventory.Domain.csproj`. Copy the shape of `Cooquoi.SharedKernel.csproj`: a plain SDK project with no packages.
  - Reference `../BuildingBlocks/Raidvahkiin.Ddd/…` and `../Cooquoi.SharedKernel/…`.
  - Add `<RootNamespace>Cooquoi.Context.Foodventory.Domain</RootNamespace>`, `<IsAotCompatible>true</IsAotCompatible>` and `<VerifyReferenceAotCompatibility>true</VerifyReferenceAotCompatibility>`.
  - The root of the new project maps to namespace `…Domain`. So `Domain/IngredientStocks/X.cs` moves to `Cooquoi.Context.Foodventory.Domain/IngredientStocks/X.cs`, and its namespace line stays as is.
- **Move with `git mv`** so history follows: every folder under `dotnet/src/Cooquoi.Context.Foodventory/Domain/` (`Common/`, `IngredientStocks/`, `Pantries/` and any others there).
- **Context project:** add a `ProjectReference` to the domain project.
- **`Cooquoi.slnx`:** add `<Project Path="src/Cooquoi.Context.Foodventory.Domain/Cooquoi.Context.Foodventory.Domain.csproj" />` next to line 5.
- **Visibility.** `StockItemEntity`'s `internal` members and `StockConsumption` are only used inside the domain; checked: no use outside `Domain/` in `src` or `tests`. So they can stay `internal`. If the build shows another use, make that one member public, or use `InternalsVisibleTo` for tests only.
- **Ids from the caller** (all `.New()` calls in this domain today):
  - `IngredientStockAggregate.cs:47`: the restock creates `StockItemId.New()` per line. Give each restock input line a `StockItemId` and let `RestockEndpoint` call `StockItemId.New()` per line.
  - `IngredientStockAggregate.cs:98`: the split in `MoveItem` creates `StockItemId.New()`. Add a `StockItemId splitId` parameter and pass `StockItemId.New()` from `MoveStockItemEndpoint`. It is unused when the whole item moves.
  - `Pantries/DefaultPantries.cs:19`: `PantryId.New()` per default. Let `For(...)` take the ids, e.g. a `Func<PantryId>` or ids from the caller. The caller is the `HouseholdCreated` handler in `Integration/`.
  - Domain tests that call these methods pass `StockItemId.New()` and `PantryId.New()`. Test code may create ids.
- **AOT flags on the building blocks:** add `IsAotCompatible` and `VerifyReferenceAotCompatibility` to `Raidvahkiin.Ddd.csproj` and `Cooquoi.SharedKernel.csproj`. If an analyzer warning appears (`IL2xxx`/`IL3xxx`), fix the code. Don't suppress the warning: the spike saw none.
- **Domain test project** `dotnet/tests/Cooquoi.Context.Foodventory.Domain.Tests/Cooquoi.Context.Foodventory.Domain.Tests.csproj`:
  - one `ProjectReference` to the domain project, and nothing else: no `Cooquoi.Testing`, no context project. xUnit and Shouldly come from `tests/Directory.Build.props`.
  - Add it to `Cooquoi.slnx` next to line 19.
  - `git mv` everything under `tests/Cooquoi.Context.Foodventory.Tests/Domain/` to the new project's root, keeping the subfolders: `Domain/IngredientStocks/X.cs` becomes `IngredientStocks/X.cs`.
  - Test namespaces become `Cooquoi.Context.Foodventory.Domain.Tests.<Folder>`.
  - If a test uses a test namespace named like a domain type, alias the type, as `dotnet/AGENTS.md` "Layout" says.
- **Shared test helper:** `Domain/IngredientStocks/TestFacts.cs` is also used by `Persistence/IngredientStockPersistenceTests.cs` (lines 29–35).
  - Move it with the domain tests.
  - Have the context test project compile the same file as a link: `<Compile Include="../Cooquoi.Context.Foodventory.Domain.Tests/IngredientStocks/TestFacts.cs" Link="Support/TestFacts.cs" />`. A test project must not reference another test project, since both are xUnit v3 executables.
  - Make the helper's namespace work in both places, e.g. by giving it a neutral namespace both import.
- **Mirrored-test rule:**
  - `MirroredTestRulesTests.MirroredPath` (lines 51–57) maps namespace `Cooquoi.Context.X.Domain.<Folders>` to `tests/Cooquoi.Context.X.Tests/Domain/<Folders>/`. Change it to `tests/Cooquoi.Context.X.Domain.Tests/<Folders after Domain>/`.
  - Until T04, only Foodventory has the new layout, so use the new path when that folder exists and the old one otherwise.
  - Mark the fallback `// shortcut: removed in E03.S01.T04`.
- **Architecture tests:** add `typeof(<any Foodventory domain type, e.g. IngredientStockAggregate>).Assembly` to `CooquoiArchitecture.OwnAssemblies`. Otherwise the error catalogue loses the `foodventory.*` codes and the mirrored-test rule loses the domain types.

## Skills

`dotnet-domain` (id rule), `dotnet-test`, `refactor`

## Model

sonnet

## Out of scope

- Foodpedia (T03), Household and the new architecture tests (T04).
- Any contract or OpenAPI change: client-generated ids in the API are E03.S02.

## Subtasks

- [x] T02.1 Create the domain project and `git mv` the folders. Add the references and the `Cooquoi.slnx` entry.
- [x] T02.2 Make the domain compile without `.New()`: restock line ids, the `MoveItem` split id, default pantry ids. Update the callers (endpoints, handler) and the domain tests.
- [x] T02.3 AOT flags on `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`; fix any analyzer warning in the code.
- [x] T02.4 Add the domain assembly to `OwnAssemblies`.
- [x] T02.5 Domain test project: create it, `git mv` the domain tests, add the `TestFacts` link in the context test project, and change `MirroredPath` with its temporary fallback. `ls dotnet/tests/Cooquoi.Context.Foodventory.Tests/Domain` fails.
- [x] T02.6 `grep -rn '\.New()\|Guid.NewGuid\|DateTime\.\|TimeProvider' dotnet/src/Cooquoi.Context.Foodventory.Domain` prints nothing.

## Done when

- `dotnet build dotnet/Cooquoi.slnx -v q` has 0 warnings.
- `dotnet format dotnet/Cooquoi.slnx --verify-no-changes` passes.
- `dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress` is all green, with the same total test count as before (318) or more, now split across the domain and context test projects.
- `pnpm nx run-many -t drift` reports `none`.

Then:
- tick the subtasks;
- write the report;
- set the status;
- commit `refactor(foodventory): domain-only assembly, domain tests and caller-supplied ids (E03.S01.T02)`;
- stop.

Status: done

Report:
Domain assembly and `Domain.Tests` created (git mv, namespaces kept); ids now caller-supplied: `NewStockLine.Id`, `MoveItem(..., splitId)`, `DefaultPantries.For(..., Func<PantryId>)`. AOT flags on `Raidvahkiin.Ddd`, `SharedKernel` and the domain: no analyzer warning.
`TestFacts` is linked into the context tests; `MirroredPath` uses the new layout with a fallback (`// shortcut: removed in E03.S01.T04`); domain assembly added to `OwnAssemblies`.
Verified: build 0 warnings, format clean, 318/318 tests, drift none, domain grep empty.
