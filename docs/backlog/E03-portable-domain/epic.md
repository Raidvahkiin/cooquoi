# E03 — Portable domain and offline-ready API

## Goal

Prepare the backend for an offline-capable mobile app without deciding its UI stack yet. Each bounded context's domain becomes its own pure assembly, which can also be compiled ahead of time (Native AOT, Avalonia, WebAssembly). Later stories make the API safe to replay from an offline queue of actions. The server stays the only author of events (spike option (a)).

## Source

- Spike [C# domain as a native library for Flutter](../../architecture/spike-flutter-native-domain.md), on branch `claude/spike-flutter-native-domain-1dc352` and not merged yet. The parts used here: §1 "What would have to move or change" (D1, D2) and §5 "Recommendation".
- [ADR 0014](../../architecture/adr/0014-backend-structure.md) §1 "Solution layout", §5 "Dependency rules"
- [ADR 0007](../../architecture/adr/0007-mobile-strategy.md) Decision

## Rules for every story

- No behaviour change visible through the API: the drift check stays `none` and every existing test keeps passing.
- Namespaces stay `Cooquoi.Context.<Name>.Domain.*`, so moved files keep their `using`s and the mirrored-test rule keeps working.
- Naming: "Foodventory context" vs "Pantry aggregate", never a bare "Pantry".

## Out of scope

- Snapshot rehydration of aggregates (spike D3).
- Shared JSON test cases.
- Decimals as strings in the contract.
- Any mobile code.
- The Native AOT proof on Android.

## Stories

| Id | Title | Status | Depends on |
|---|---|---|---|
| [E03.S01](S01-domain-assemblies/story.md) | Domain-only assemblies per context | done | E02.S05 |
| E03.S02 | Offline-ready contract: client-generated ids, `occurredOn`, batch put-away, writes return state, stock snapshot with ETag | todo (to plan) | E03.S01 |

E02.S06 depends on E03.S01: S06 grows the IngredientStock aggregate, and moving files under a story in progress causes conflicts.

## Open questions

| # | Question | Default (**Decided** when confirmed) |
|---|---|---|
| Q1 | Folder of the domain project: sibling `src/Cooquoi.Context.<Name>.Domain/`, or grouped `src/Contexts/<Name>/…`? | Sibling, with no move of the existing projects |
| Q2 | Domain tests: keep them in `Cooquoi.Context.<Name>.Tests/Domain/`, or add `Cooquoi.Context.<Name>.Domain.Tests`? | New `tests/Cooquoi.Context.<Name>.Domain.Tests/` per context, mirroring the source structure; domain tests move there; the mirrored-test rule follows — **Decided 2026-11-27** |
| Q3 | Check the AOT compatibility of the domain assemblies in CI with a real `dotnet publish -p:PublishAot=true`, or only with the `IsAotCompatible` analyzers (warnings as errors)? | Analyzers only. A real publish needs `clang` on the runner, which comes with the native proof |
