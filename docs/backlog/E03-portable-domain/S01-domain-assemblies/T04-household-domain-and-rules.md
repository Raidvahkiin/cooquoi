# E03.S01.T04 — Household domain assembly and domain test project, and the assembly-level architecture tests

## Goal

`Cooquoi.Context.Household.Domain` holds what was `Cooquoi.Context.Household/Domain/`, and its unit tests move to `Cooquoi.Context.Household.Domain.Tests`. The mirrored-test rule drops its temporary fallback. Two new architecture tests lock in ADR 0023:
- each domain assembly references only `Raidvahkiin.Ddd`, `Cooquoi.SharedKernel` and the BCL;
- no domain type calls an id factory, `Guid.NewGuid` or the clock.

## Read only

- `docs/architecture/adr/0023-domain-only-assemblies.md` Decision
- `dotnet/src/Cooquoi.Context.Foodventory.Domain/Cooquoi.Context.Foodventory.Domain.csproj` (from T02): copy it
- `dotnet/tests/Cooquoi.Architecture.Tests/DependencyRulesTests.cs:1-40`: test style (ArchUnitNET, `CooquoiArchitecture.Value`)
- `dotnet/tests/Cooquoi.Architecture.Tests/CooquoiArchitecture.cs:17-38`
- `dotnet/AGENTS.md` "Hard rules"

## Reference

- **Household split:** same steps as T02 and T03. The domain has no `.New()` call today (checked), so there is no id change.
- **Domain test project:** same steps as T02, in `dotnet/tests/Cooquoi.Context.Household.Domain.Tests/`. The Household domain tests use no `Support/` helper (checked), so there's nothing to link.
- **Mirrored-test rule:** remove the T02 fallback (`// shortcut: removed in E03.S01.T04`). Every context now uses `tests/Cooquoi.Context.<Name>.Domain.Tests/<Folders>/`.
- **Test 1, `Domain_assemblies_reference_only_the_building_blocks_and_the_shared_kernel`:**
  - For each assembly in `OwnAssemblies` whose name matches `^Cooquoi\.Context\.\w+\.Domain$`, call `assembly.GetReferencedAssemblies()`.
  - Assert every name is `Raidvahkiin.Ddd`, `Cooquoi.SharedKernel`, or a BCL assembly (`System.*`, `netstandard`, `mscorlib`).
  - Also assert exactly three such assemblies are found, so a context with no domain project can't pass by being absent.
  - In the same test, assert each `Cooquoi.Context.*.Domain.Tests` project references only its own domain project. Read the `.csproj` files from `CooquoiArchitecture.SolutionDirectory`: three csproj files, one `ProjectReference` each.
- **Test 2, `Domain_never_creates_ids_or_reads_the_clock`:**
  - Use ArchUnitNET method dependencies: types in namespaces matching `^Cooquoi\.Context\.\w+\.Domain(\..+)?$` must not call `System.Guid.NewGuid`, `System.DateTime.get_Now`, `get_UtcNow`, `get_Today`, `System.DateTimeOffset.get_Now` or `get_UtcNow`, `System.TimeProvider`, or any method named `New` on a type whose name ends in `Id`.
  - The `New()` factory is declared on the id types themselves, so exclude the declaring type: the rule bans calling it, not defining it.
  - If ArchUnitNET can't express a call rule, use a reflection and IL-free fallback: scan the source of `dotnet/src/Cooquoi.Context.*.Domain/**/*.cs` for the patterns `\w+Id\.New\(\)`, `Guid\.NewGuid`, `DateTime\.(Now|UtcNow|Today)`, `DateTimeOffset\.(Now|UtcNow)` and `TimeProvider`, as `CooquoiArchitecture.SourceRoot` already does for other source scans (see the `ExecuteUpdate` rule in `dotnet/AGENTS.md` "Hard rules").
- **`dotnet/AGENTS.md` "Hard rules":** add one line for each new test.

## Skills

`dotnet-test`, `refactor`

## Model

sonnet

## Out of scope

- A native publish in CI (Q3).

## Subtasks

- [ ] T04.1 Household domain project, `git mv`, references, `Cooquoi.slnx`, `OwnAssemblies`.
- [ ] T04.1b Household domain test project, `git mv` the domain tests, and remove the `MirroredPath` fallback.
- [ ] T04.2 Test 1 (assembly references). Check it fails if a domain csproj gets a `PackageReference`, then revert that.
- [ ] T04.3 Test 2 (no ids or clock). Check it fails on a temporary `PantryId.New()` in a domain file, then revert that.
- [ ] T04.4 The `dotnet/AGENTS.md` "Hard rules" lines.

## Done when

- `dotnet build dotnet/Cooquoi.slnx -v q` has 0 warnings.
- `dotnet format dotnet/Cooquoi.slnx --verify-no-changes` passes.
- The full `dotnet test` is green, with 2 more tests than before.
- `pnpm nx run-many -t lint typecheck test biome-check knip build drift` is green, with drift `none`.

Then:
- tick the subtasks;
- write the report;
- set the status;
- commit `refactor(household): domain-only assembly, domain tests and domain assembly architecture tests (E03.S01.T04)`;
- stop.

Status: todo

Report:
