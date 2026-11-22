# E02.S01 — Foodpedia expiry kind and published API

## Goal

Every Foodpedia ingredient has an expiry kind (`printed` | `estimated` | `long-life`). It is seeded, required on create, shown in the detail and changed by curators through `:set-expiry-kind`, which raises `IngredientExpiryKindChanged`. The Foodpedia context also publishes what the Foodventory context needs in-process: P1 `GetIngredients`, P2 `NormalizeQuantity`, P3 `ProposeShelfLife`, and the integration events it consumes.

## Acceptance

- [x] F12: create without `expiryKind` is a 400. `GET /v1/ingredients/{id}` returns `expiryKind`. All seed ingredients carry one.
- [x] C11: `POST /v1/ingredients/{id}:set-expiry-kind` (curator, `If-Match`) returns 204 + ETag and raises `IngredientExpiryKindChanged` once. The same kind again raises nothing. A retired ingredient gives 422 `foodpedia.ingredient-retired` (F10).
- [x] P1–P3 exist under `Cooquoi.Context.Foodpedia/PublicApi/` with unit and integration tests. P2 applies universal factors, then Foodpedia conversions, else unnormalized. P3 returns `from + days` and the recommended storage, or none.
- [x] Integration events for `IngredientExpiryKindChanged`, `ConversionDefined` and `IngredientRetired` are published from `PublicApi/`, so the Foodventory context never references Foodpedia `Domain/`.
- [x] Drift: `drift: none`. Error-catalogue test green. Webapp typecheck and tests green after regeneration.

## Context

- [Foodpedia domain model](../../../contexts/foodpedia/domain-model.md) "ExpiryKind (enum)", "Invariants" (F10, F12), "Domain events", "Seed data"
- [Foodpedia use cases](../../../contexts/foodpedia/use-cases.md) C11, "Published API for other contexts (in-process)"
- `dotnet/src/Cooquoi.Context.Foodpedia/Domain/Ingredients/IngredientAggregate.cs:19-73`
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/DefineConversion/DefineConversionEndpoint.cs:16-31` (replace-action reference)
- `contracts/contexts/foodpedia/models.tsp:16,53-67,94`, `routes.tsp:35-38`
- Open question Q9 ([epic](../epic.md#open-questions))

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-contract-expiry-kind.md) | Contract: expiry kind and `:set-expiry-kind` | done | — | yes | subagent (haiku) |
| [T02](T02-domain-persistence-seed.md) | Domain, persistence, migration and seed for the expiry kind | done | — | yes | subagent (sonnet) |
| [T03](T03-endpoints.md) | Create, detail and `:set-expiry-kind` endpoints | done | T01, T02 | no | subagent (sonnet) |
| [T04](T04-public-api.md) | Foodpedia published API (P1–P3) and integration events | done | T02 | yes (with T03) | subagent (opus) |

T01 (TypeSpec and TypeScript) and T02 (C# domain and persistence) touch disjoint files. T03 (`Features/`) and T04 (`PublicApi/`, `ConversionTable`) are disjoint too. The story review runs after T04.

## Summary

Done 2026-11-22 in 6 commits (9af60de..517a558, plus this one). The contract has `ExpiryKind` and `:set-expiry-kind`. The domain, a PascalCase `expiry_kind` column, the migration and the seed are in place, and the API speaks kebab case through `ContractEnumNames`.
`PublicApi/` holds `IFoodpediaCatalog` (P1–P3, primitives only) and three integration events, which `Integration/IntegrationEventsHandler` publishes from the domain events.
Verified: Foodpedia 106, Architecture 17 and Platform 14 tests pass, `drift: none`, and the webapp typecheck and tests pass. The `dotnet-review` blocker count is 0, and its should and nits are fixed in 517a558.
Watch: Wolverine tracked-session tests are order-sensitive. An integration test for "same kind publishes nothing" made `DefineConversion` time out in the full suite, so that rule stays covered by the aggregate test only.
Usage: weekly all-models was 75% before the review.
