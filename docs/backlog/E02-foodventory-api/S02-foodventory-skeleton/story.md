# E02.S02 — Foodventory context skeleton, current household, default Pantry aggregates

## Goal

`Cooquoi.Context.Foodventory` exists in the ADR 0014 structure: module, schema `foodventory`, `FoodventoryDbContext` with the Wolverine outbox, and architecture rules. The Household context publishes `HouseholdCreated` (with the creator's locale) and a published query that gives the caller's household id. When the Foodventory context receives `HouseholdCreated`, it creates the default Pantry aggregates Fridge, Freezer and Dry goods in the creator's language.

## Acceptance

- [x] First sign-in (`POST /v1/me:sign-in` for a new user) publishes `HouseholdCreated` once, in the same transaction as the household (outbox).
- [x] `Household/PublicApi` resolves the caller's household id from issuer + subject.
- [x] The Foodventory context has `Integration/CurrentHousehold`, which every later endpoint uses (FV2). With no household: open question Q2.
- [x] Default Pantry aggregates: `en` Fridge (`fridge`) / Freezer (`freezer`) / Dry goods (`room`); `fr` Réfrigérateur / Congélateur / Épicerie sèche; `vi` Tủ lạnh / Tủ đông / Đồ khô. Any other locale falls back to English. The handler is idempotent (FV14 spirit: a second delivery creates nothing).
- [x] FV1 is enforced in `PantryName` (1–60 trimmed) and by a unique index on a case- and accent-insensitive key per household.
- [x] Contract: the `Cooquoi.Foodventory` namespace with the pantry operations (C1–C4, Q1) and their error codes, so E02.S03 only implements them.
- [x] Architecture tests: Foodventory in `OwnAssemblies`; contexts still never reference each other, except `Integration/` → another context's `PublicApi/`.

## Context

- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "Pantry (aggregate root)", FV1, FV2, "Persistence notes"
- [ADR 0014](../../../architecture/adr/0014-backend-structure.md) (only if `dotnet/AGENTS.md` "Layout" does not answer)
- `dotnet/src/Cooquoi.Context.Household/Features/SignIn/UserSignIn.cs:31-58`, `HouseholdModule.cs:21`
- `dotnet/src/Cooquoi.Context.Foodpedia/FoodpediaModule.cs:20-48`, `dotnet/src/Cooquoi.Api/Program.cs:31-67`
- `dotnet/tests/Cooquoi.Architecture.Tests/CooquoiArchitecture.cs:17-25`, `DependencyRulesTests.cs:31-39`
- Open questions Q2, Q3 ([epic](../epic.md#open-questions))

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-household-created.md) | Household: `HouseholdCreated` through the outbox and a current-household query | done | E02.S01.T04 (convention) | yes | s02-t01 |
| [T02](T02-context-skeleton.md) | Foodventory context skeleton and architecture rules | done | E02.S01.T04 | yes | subagent (opus) |
| [T03](T03-contract-pantries.md) | Contract: Foodventory namespace, Pantry aggregate operations and errors | done | — | yes | subagent (sonnet) |
| [T04](T04-default-pantries.md) | Pantry aggregate and default Pantry aggregates on `HouseholdCreated` | done | T01, T02, T03 | no | subagent (sonnet) |

T01 (Household context), T02 (new project, `Program.cs`, architecture tests) and T03 (TypeSpec, webapp messages) touch disjoint files and can run together. T01 only needs the `PublicApi/` convention from E02.S01.T04; it can start from that task's report.

## Summary

Done 2026-11-26: `ae2adc4` (T01), `92da37a` (T03), `0d0b049` (T02), `93b98c5` (T04), plus review fixes. .NET suite 198/198, `nx test` green.
Sign-in publishes `HouseholdCreatedEvent` through the outbox (Lightweight middleware re-scrape is avoided by clearing tracked events after the outbox save); `IHouseholdDirectory` gives the household; with no household `CurrentHousehold` throws `common.not-found` (Q2).
`nx drift` stays red until E02.S03: the contract declares the four pantry routes the backend does not serve yet.
Open for S03: `HouseholdCreatedHandler` check-then-insert races on overlapping redelivery (the unique index rejects the loser as `foodventory.name-taken` and the message retries); add a transaction + advisory lock or a discard policy. `invalid-name` is 422 (BusinessRule), as in Foodpedia.
