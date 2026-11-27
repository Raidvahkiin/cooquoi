# E02 — Foodventory API: manual stock

## Goal

First release of the Foodventory context, API only: a household restocks by hand, puts items away into its Pantry aggregates, consumes and corrects them, and reads its stock. The Planning & Suggestion context can then read real stock through the published read model (in-process and over HTTP) before any UI exists. Roadmap step 1 ([README](../README.md#roadmap-after-e01)).

## Source

- [Foodventory domain model](../../contexts/foodventory/domain-model.md) (whole: FV1–FV15, "Decided (2026-11-22)")
- [Foodventory use cases](../../contexts/foodventory/use-cases.md): Queries, Commands — pantries, Commands — stock, Published API for Planning & Suggestion
- [Foodpedia domain model](../../contexts/foodpedia/domain-model.md) and [use cases](../../contexts/foodpedia/use-cases.md): the expiry kind only (F12, C11, `IngredientExpiryKindChanged`, seed), plus the published API P1–P3 that the Foodventory context calls

## Rules for every story

- API first: no webapp screens or Server Actions. Contract tasks still regenerate the client and add the error messages to `ts/apps/cooquoi-webapp/messages/{en,fr,vi}.json` (the error-catalogue test checks them), nothing more on the webapp side.
- Contract first: a story that adds operations starts with its contract task (`contract` skill); the pantry operations are declared in S02.T03, before the Pantry aggregate, so S03 has none. Error codes are `foodventory.*` (and `foodpedia.*`). The drift check goes green in the story's backend task.
- Domain rules use FV1–FV15 ids (`dotnet-domain`). Endpoints use `dotnet-feature`. Tests use `dotnet-test`: domain tests named after rule ids, and Alba + Testcontainers integration tests.
- Enum values are camelCase in the contract, JSON, database and published API ([ADR 0021](../../architecture/adr/0021-enum-values-camel-case.md)): `longLife`, `noThreshold`. Routes, actions and error codes stay kebab-case; `long-life` in the briefs names the concept, not the code.
- Serialized property names (JSON, jsonb, events, seed) are camelCase; C# members PascalCase, columns snake_case ([ADR 0022](../../architecture/adr/0022-property-names-camel-case.md)).
- Events: domain events `<Fact>DomainEvent` in `Domain/<Aggregate>/Events/`, integration events `<Fact>Event` in `PublicApi/Events/` (`dotnet/AGENTS.md` "Domain"). Sign-in is `POST /v1/me:sign-in`; `GET /v1/me` is read-only. One `InitialFoodventory` migration, regenerated on each model change until first release.
- Naming: "Foodventory context" vs "Pantry aggregate", never a bare "Pantry".

## Out of scope

Receipt or product scan (`ReceiptScanned`), `MealCooked` consumption (no stub), reading printed dates from photos, suggesting a kind override automatically, `ExpiryWatch` / `StockItemExpiringSoon` (see open questions), any UI, and the curator app (E01.S06, deferred).

## Stories

| Id | Title | Status | Depends on |
|---|---|---|---|
| [E02.S01](S01-foodpedia-expiry-kind/story.md) | Foodpedia expiry kind and published API | done | — |
| [E02.S02](S02-foodventory-skeleton/story.md) | Foodventory context skeleton, current household, default Pantry aggregates | done | E02.S01 (T04) |
| [E02.S03](S03-pantry-commands/story.md) | Pantry aggregate commands and list | done | E02.S02 |
| [E02.S04](S04-restock-put-away/story.md) | IngredientStock aggregate: restock and put away | done | E02.S01, E02.S03 |
| [E02.S05](S05-consume-correct/story.md) | Consume, discard, adjust, set expiry | done | E02.S04 |
| [E02.S06](S06-thresholds-conversions-expiry-kind/story.md) | Thresholds, household conversions, expiry kind override, remembered shelf life | todo | E02.S05, E03.S01 |
| [E02.S07](S07-stock-queries/story.md) | Stock queries | todo | E02.S06 |
| [E02.S08](S08-planning-read-model/story.md) | Planning read model and verification under Aspire | todo | E02.S07 |

Requested steps (6) and (7) are merged into S06: they change the same aggregate methods and the same contract file, so splitting them would mean two sessions editing the same code one after the other. S01.T01–T03 can run while S02.T01 runs (Household context only).

## Open questions

Raised while planning (2026-11-22) and referenced from each story's Context. Record the answer in the last column, or keep the default.

| # | Question | Default (**Decided** when confirmed) |
|---|---|---|
| Q1 | How does a curl/.http script get a token for dev@cooquoi.local? Both Keycloak clients have `directAccessGrantsEnabled: false`. | A dev-only public client `cooquoi-dev-cli` with the password grant, in the dev realm import only (S08.T03) — **Decided 2026-11-22** |
| Q2 | A caller with no household yet (never called `POST /v1/me:sign-in`): what do Foodventory endpoints answer? | 404 `common.notFound` for the household; the script calls `POST /v1/me:sign-in` first |
| Q3 | The dev Postgres has a data volume, so existing households never got `HouseholdCreated`. Backfill the default Pantry aggregates? | No backfill: reset the dev volume before S03 verification (production has no households yet) — **Decided 2026-11-22** |
| Q4 | Error codes for shape rules the domain doc does not list (pantry name length, quantity amount, conversion amounts) | Add `foodventory.invalidName`, `foodventory.invalidQuantity`, `foodventory.invalidConversionAmount` (Validation), mirroring `foodpedia.*`, and add them to the domain doc's error list |
| Q5 | Routes for Q5, Q6 and the summary, and the delete operation (no template exists) | `GET /v1/stock-items/expiring`, `GET /v1/stock/low`, `GET /v1/stock/summary` (guid constraint on `{ingredientId}`); `DELETE /v1/pantries/{id}` with `If-Match` via a new `DeleteOperation` template |
| Q6 | Threshold, override or conversion on an ingredient with no IngredientStock aggregate yet: `If-Match` is impossible | Create it on the fly, `If-Match` required only when it exists — **Decided 2026-11-22** |
| Q7 | Q2/Q3 `tags` filter needs Foodpedia tags across contexts | Deferred: no `tags` filter in E02 |
| Q8 | `expiryUnusual`: "far outside the household's usual range" has no rule | Always `false` in E02; the field is in the contract |
| Q9 | Foodpedia events `BaseUnitChanged`, `ConversionRemoved` and `IngredientRestored` do not exist yet (no such commands) | Handlers only for `ConversionDefined`, `IngredientExpiryKindChanged` and `IngredientRetired`; the others come with their Foodpedia commands |
| Q10 | `ExpiryWatch` (daily `StockItemExpiringSoon`) | Not in E02; Q5 and the read model cover "expiring soon" for Planning |
| Q11 | Over HTTP the summary is "the same view as Q3" in the use cases. A dedicated summary route, or Q3 extended? | A dedicated `GET /v1/stock/summary` returning `StockSummary` exactly; Q3 stays the browse view — **Decided 2026-11-22** |
