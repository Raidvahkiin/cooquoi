# E03.S01.T01 — ADR 0023, ADR 0014 amendment, AGENTS.md and skill rules

## Goal

Record the decision "one domain-only assembly per bounded context, portable (AOT-compatible), with no ambient inputs" as ADR 0023. Amend ADR 0014 to point to it, and write the rules where workers read them (`dotnet/AGENTS.md`, `dotnet-domain` skill).

## Read only

- `docs/architecture/adr/0014-backend-structure.md` §1 "Solution layout" (lines 13–49) and §5 "Dependency rules" (lines 114–123)
- `docs/architecture/adr/0022-property-names-camel-case.md`: the format of a short recent ADR (copy its headings)
- `docs/README.md`: the ADR table (lines 30–51)
- `dotnet/AGENTS.md` "Layout" (lines 17–47), "Hard rules" (49–60), "Domain" (110–125)
- `.agents/skills/dotnet-domain/SKILL.md` "Do not" (lines 36–43)

## Reference

These are the facts to put in ADR 0023. Do not re-derive them.

**Context:**
- The user wants an offline-capable mobile app later. ADR 0007 names Flutter, and Avalonia is being considered.
- The domain is already pure by namespace (`DependencyRulesTests.cs:11`), but it lives in the same project as EF Core, Wolverine and ASP.NET, so nothing can reference it alone.
- A spike (branch `claude/spike-flutter-native-domain-1dc352`, `docs/architecture/spike-flutter-native-domain.md`) compiled the Foodventory domain with Native AOT and got 0 trim or AOT warnings. FV7 passed through an exported C entry point.

**Decision:**
1. One project per context, `Cooquoi.Context.<Name>.Domain`, in a sibling folder `src/Cooquoi.Context.<Name>.Domain/`.
   - It holds what was `Domain/`, with the namespaces unchanged.
   - It references only `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`.
   - The context project references it.
2. `IsAotCompatible` and `VerifyReferenceAotCompatibility` are on for the domain projects, `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`. CI checks this through the analyzers (warnings as errors), not through a native publish.
3. No ambient inputs in the domain:
   - Ids come from the caller. The `<X>Id.New()` factories stay, but only endpoints and handlers call them.
   - Time comes as a value (`today`, `occurredOn`). Never `TimeProvider` or `DateTime.*` in the domain.
4. No I/O, async or DI in the domain. Facts from other contexts arrive as values, like `FoodpediaFacts`.
5. Tests mirror the source structure: one `tests/Cooquoi.Context.<Name>.Domain.Tests/` per domain project, referencing only that project. It holds the domain (unit) tests, in the same folders as the types they test. `Cooquoi.Context.<Name>.Tests` keeps the integration tests (Features, Persistence, Integration, PublicApi).
6. Architecture tests check the assembly references and the absence of id or time calls in the domain.

**Consequences:**
- Three more projects, and one more project per new context.
- The domain can be compiled for Native AOT, Avalonia or WebAssembly without changes.
- Purity is enforced by the compiler.
- Domain test builds are faster.

**Not decided (deferred, with the spike as source):**
- snapshot rehydration (spike D3);
- shared JSON test cases;
- decimals as strings in the contract;
- the mobile stack itself;
- sync option (a), "the server re-runs queued intents", which goes into a later offline ADR.

In ADR 0014, add a one-line note under §1 and §5: "Amended by ADR 0023: each context's domain is its own `Cooquoi.Context.<Name>.Domain` project." Update the layout tree in §1 to show `Cooquoi.Context.Foodventory.Domain/` next to the context project.

## Skills

none (docs only)

## Model

sonnet

## Out of scope

- Any `.cs` or `.csproj` change (T02–T04).
- An offline or mobile ADR. This one is limited to the domain assemblies.

## Subtasks

- [ ] T01.1 `docs/architecture/adr/0023-domain-only-assemblies.md`: Status `Accepted`, with Context, Decision, Consequences and Alternatives. Alternatives considered: keep one project with the namespace rule only (rejected: not referenceable, no AOT analyzers); one shared `Cooquoi.Domain` for all contexts (rejected: merges the contexts' models).
- [ ] T01.2 ADR 0014 §1 tree and the §1 and §5 notes; `docs/README.md` ADR table row for 0023.
- [ ] T01.3 `dotnet/AGENTS.md` (also the "tests/" part of the Layout tree and the Naming line: `Cooquoi.Context.<Name>.Domain.Tests/` for unit tests, `Cooquoi.Context.<Name>.Tests/` for `Features/`, `Persistence/`, `Integration/`, `PublicApi/`):
  - "Layout": add `Cooquoi.Context.<Name>.Domain/` and say that `Domain/` moved there.
  - "Domain": add a "Portable" row with no ambient ids or time (callers pass `<X>Id.New()` and `today`), no I/O, async or DI, and AOT-compatible.
  - Replace "Time: `TimeProvider`" with "Time: passed in as a value; `TimeProvider` only in endpoints and handlers".
- [ ] T01.4 `.agents/skills/dotnet-domain/SKILL.md` "Do not": add "Call `<X>Id.New()`, `Guid.NewGuid()` or read the clock inside the domain: take the id or date as a parameter".

## Done when

`pnpm nx run-many -t biome-check` is green, if docs are in its scope. Then `grep -n 0023 docs/README.md docs/architecture/adr/0014-backend-structure.md dotnet/AGENTS.md` shows the index row and the amendment notes.

Then:
- tick the subtasks;
- write the report;
- set the status;
- commit `docs(adr): 0023 domain-only assemblies per context (E03.S01.T01)`;
- stop.

Status: todo

Report:
