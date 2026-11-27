# ADR 0023 — One portable, domain-only assembly per bounded context

- **Status**: Accepted
- **Date**: 2026-11-27

## Context

An offline-capable mobile app is planned. [ADR 0007](0007-mobile-strategy.md) names Flutter as the native target, and Avalonia is being considered. Either must run the domain rules on the device.

The domain is already pure by namespace (`DependencyRulesTests`: `Domain/` uses only `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`), but it lives in the same project as EF Core, Wolverine and ASP.NET, so nothing can reference it alone, and the compiler cannot enforce its purity.

A spike (branch `claude/spike-flutter-native-domain-1dc352`, `docs/architecture/spike-flutter-native-domain.md`) compiled the Foodventory context's domain with Native AOT and got 0 trim or AOT warnings. Rule FV7 passed through an exported C entry point.

| Option | Verdict |
|---|---|
| **One `Cooquoi.Context.<Name>.Domain` project per context, portable, no ambient inputs** | **Chosen**: referenceable alone, purity checked by the compiler and the AOT analyzers |
| Keep one project per context with the namespace rule only | Rejected: not referenceable alone, no AOT analyzers on the domain |
| One shared `Cooquoi.Domain` for all contexts | Rejected: merges the contexts' models |

## Decision

1. One project per context, `Cooquoi.Context.<Name>.Domain`, in a sibling folder `src/Cooquoi.Context.<Name>.Domain/`.
   - It holds what was `Domain/`, with the namespaces unchanged.
   - It references only `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`.
   - The context project references it.
2. `IsAotCompatible` and `VerifyReferenceAotCompatibility` are on for the domain projects, `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`. CI checks this through the analyzers (warnings as errors), not through a native publish.
3. No ambient inputs in the domain:
   - Ids come from the caller. The `<X>Id.New()` factories stay, but only endpoints and handlers call them.
   - Time comes as a value (`today`, `occurredOn`). Never `TimeProvider` or `DateTime.*` in the domain.
4. No I/O, async or DI in the domain. Facts from other contexts arrive as values, like `FoodpediaFacts`.
5. Tests mirror the source structure: one `tests/Cooquoi.Context.<Name>.Domain.Tests/` per domain project, referencing only that project. It holds the domain (unit) tests, in the same folders as the types they test. `Cooquoi.Context.<Name>.Tests` keeps the integration tests (`Features/`, `Persistence/`, `Integration/`, `PublicApi/`).
6. Architecture tests check the assembly references and the absence of id or time calls in the domain.

## Consequences

- Three more projects now, and one more project per new context.
- The domain can be compiled for Native AOT, Avalonia or WebAssembly without changes.
- Purity is enforced by the compiler, not only by a namespace test.
- Domain test builds are faster.

**Not decided here** (deferred, with the spike as source): snapshot rehydration (spike D3), shared JSON test cases, decimals as strings in the contract, the mobile stack itself, and sync option (a), "the server re-runs queued intents", which goes into a later offline ADR.
