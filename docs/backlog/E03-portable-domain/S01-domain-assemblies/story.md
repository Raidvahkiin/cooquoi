# E03.S01 — Domain-only assemblies per context

## Goal

The `Domain/` folder of each bounded context (Foodventory, Foodpedia, Household) moves into its own project, `Cooquoi.Context.<Name>.Domain`. That project references only `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`. All three projects and the two building blocks build with the AOT analyzers on. Domain methods no longer create ids themselves: the caller passes them in. An ADR records the decision and amends ADR 0014.

## Acceptance

- [x] ADR 0023 "Domain-only assemblies per context" is accepted and indexed in `docs/README.md`. ADR 0014 §1 and §5 point to it.
- [x] `src/Cooquoi.Context.{Foodventory,Foodpedia,Household}.Domain/*.csproj` exist, are listed in `Cooquoi.slnx`, and are referenced by their context project. Namespaces are unchanged.
- [x] `IsAotCompatible` and `VerifyReferenceAotCompatibility` are `true` on the three domain projects, `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`. The build has 0 warnings.
- [x] No `<X>Id.New()`, `Guid.NewGuid()`, `DateTime.*`, `DateTimeOffset.*` or `TimeProvider` call is left under any `*.Domain` project. Today these calls are in `IngredientStockAggregate` (restock, and the split in `MoveItem`), `DefaultPantries` and `TagAggregate.Create`. Ids come from the caller (endpoint or handler).
- [x] An architecture test checks that each `Cooquoi.Context.*.Domain` assembly references only `Raidvahkiin.Ddd`, `Cooquoi.SharedKernel` and the BCL. Another fails on any `.New()` or `Guid.NewGuid()` call inside a domain assembly.
- [x] `tests/Cooquoi.Context.{Foodventory,Foodpedia,Household}.Domain.Tests/` exist, are listed in `Cooquoi.slnx`, and reference only their domain project (plus the test packages from `tests/Directory.Build.props`). Every test of a domain type moves there, with the same folder below the project root as its type: `IngredientStocks/…Tests.cs`. No `Domain/` folder is left in `Cooquoi.Context.<Name>.Tests`.
- [x] The mirrored-test rule (`MirroredTestRulesTests.MirroredPath`) expects `tests/Cooquoi.Context.<Name>.Domain.Tests/<Aggregate>/<Type>Tests.cs`.
- [x] The error catalogue and the mirrored-test rule still see every domain type, because the domain assemblies are in `CooquoiArchitecture.OwnAssemblies`.
- [x] `dotnet/AGENTS.md` "Layout" and "Domain", and the `dotnet-domain` skill, state the portability rules: no ambient inputs (ids, time), and no I/O, async or DI.
- [x] Full verification is green with no API change: build, format, every `dotnet test`, and `pnpm nx run-many -t lint typecheck test biome-check knip build drift` with drift `none`.

## Context

- [Spike](../../../architecture/spike-flutter-native-domain.md) §1 "Dependencies" and "What would have to move or change", on branch `claude/spike-flutter-native-domain-1dc352`. The facts needed here are copied into the task files.
- [ADR 0014](../../../architecture/adr/0014-backend-structure.md) §1 "Solution layout", §5 "Dependency rules"
- `dotnet/AGENTS.md` "Layout", "Hard rules", "Domain"
- `dotnet/tests/Cooquoi.Architecture.Tests/CooquoiArchitecture.cs:17-28` (`OwnAssemblies`), `DependencyRulesTests.cs:11-16`, `MirroredTestRulesTests.cs:51-57` (`MirroredPath`)

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-adr-and-rules.md) | ADR 0023, ADR 0014 amendment, AGENTS.md and skill rules | done | — | yes | subagent-t01 |
| [T02](T02-foodventory-domain.md) | Foodventory domain assembly and domain test project, caller-supplied ids, AOT flags on the building blocks | done | T01 | no | subagent-t02 |
| [T03](T03-foodpedia-domain.md) | Foodpedia domain assembly and domain test project, caller-supplied `TagId` | done | T02 | no | subagent-t03 |
| [T04](T04-household-domain-and-rules.md) | Household domain assembly and domain test project, and the assembly-level architecture tests | done | T03 | no | subagent-t04 |

T02–T04 all edit `Cooquoi.slnx`, `CooquoiArchitecture.cs` and `MirroredTestRulesTests.cs`, so they run in order. T01 changes docs only.

## Summary

Each context's `Domain/` is now its own AOT-compatible `Cooquoi.Context.<Name>.Domain` project, with a mirroring `….Domain.Tests` project, and the namespaces are unchanged (ADR 0023, with amendments to ADR 0014). Ids are supplied by the caller: `NewStockLine.Id`, the `splitId` of `MoveItem`, `DefaultPantries.For(…, Func<PantryId>)` and `TagAggregate.Create(TagId, …)`.
Two new architecture tests check the domain assembly references and the absence of id or clock calls. The story review found that the BCL half of the second rule was vacuous, because CoreLib was not loaded. c87febb fixes that, and allows `Guid.CreateVersion7` only in the `<X>Id` types. Probes confirm the rule now fails on violations.
Verified: build has 0 warnings, format is clean, 320/320 tests pass (318 + 2), the full Nx run is green and drift is `none`.
Left: `MoveItem` has 6 parameters (it had 5 before this story), and `DefaultPantries.For` has 3. The review listed both as nits and they are unchanged; they are candidates for E02.S06, which grows the IngredientStock aggregate.
