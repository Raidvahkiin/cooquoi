---
name: dotnet-feature
description: Implement a use case in a Cooquoi C# bounded context end to end — request and validator, Wolverine.HTTP endpoint (Load, If-Match, ETag, 201/204), queries with projections, sorting and paging, EF Core mapping, unique constraints, migrations, projections and event handlers, module registration and tests. Use when asked to add or change a backend endpoint, query, command or event reaction.
---

# .NET feature

Conventions: `dotnet/AGENTS.md`. Reference implementation: `dotnet/src/Cooquoi.Context.Foodpedia/` (`Features/DefineConversion/` for an action, `Features/CreateIngredient/` for a create, `Features/SearchIngredients/` for a list). Use cases come from `docs/contexts/<context>/use-cases.md` (Q1, C5…): read only the entries you implement. Read the reference feature that matches your kind, not the whole context.

## Steps

1. **Contract first** (`contract` skill). Find or add the operation in `contracts/` (route, request, response, error codes). If the contract is not written yet, write it in the same change or list what it must declare.
2. **Domain.** New or changed rules → `dotnet-domain` skill.
3. **Request + validator** — `Features/<UseCase>/<UseCase>.cs`: a `sealed record` (nested records for inputs) and `<UseCase>Validator : AbstractValidator<…>` for shape only (`NotEmpty`, `MaximumLength`, one-of with `.WithErrorCode("validation.oneOf")`). The validator runs automatically and answers 400 in the contract shape.
   - Query strings: a positional record whose parameters carry `[FromQuery(Name = "camelName")]`, bound with `[AsParameters]`. Paging fields `int?`, defaulted in the endpoint. **Not** init-only properties (Wolverine codegen fails at runtime).
4. **Endpoint** — `Features/<UseCase>/<UseCase>Endpoint.cs`, `public static class`:
   ```csharp
   public static async Task<IngredientAggregate> LoadAsync(Guid ingredientId, FoodpediaDbContext db, CancellationToken cancellationToken) =>
       await db.Ingredients.FindAsync([new IngredientId(ingredientId)], cancellationToken)
           ?? throw CommonError.NotFound.ToException(("resource", "Ingredient"), ("id", ingredientId));

   [WolverinePost("/v1/ingredients/{ingredientId}:define-conversion", OperationId = "defineConversion")]
   [Authorize(Policy = CooquoiPolicies.Curator)]
   [EmptyResponse]
   public static void Post(DefineConversion request, IngredientAggregate ingredient, HttpContext http)
   {
       IfMatch.Require(http.Request, ingredient);          // replace-style actions only
       IfMatch.SetETagAfterSave(http, ingredient);          // 204 answers the new version
       ingredient.DefineConversion(new Conversion(Unit.FromCode(request.Unit)!, request.UnitAmount, request.BaseAmount));
   }
   ```
   - Route parameter names match `LoadAsync` parameters. `OperationId` = the contract's operation name.
   - Create: pre-checks (Lookups), `db.<Set>.Add(aggregate)`, `return TypedResults.Created($"/v1/…/{id}", new Created<Name>(id));`.
   - **No `SaveChanges`**, no `try/catch`, no error responses: Wolverine saves after the endpoint, the exception handler maps every `DomainException`. Idempotency-Key and concurrency retries are handled by Platform — nothing to add.
   - ≤ 80 lines per file; ≤ 30 per method. Long pre-checks move to a private method or a Lookup.
5. **Queries** (Q-use cases): `AsNoTracking`, `Select` into an internal row class with `required init` members (orderable in SQL), explicit `Where` per filter, `SortMap<T>` whitelist with a default sort and a tie-breaker, `ToPageAsync`, then `page.Map(row => row.ToSummary())`. Never load an aggregate to read it. Names in a requested locale: pick the locale or English in the projection. Read endpoints also set `IfMatch.SetETag(response, version)`.
6. **Persistence** — `Persistence/Configurations/<X>Configuration.cs`: map new state (table in AGENTS.md: child table / jsonb / `text[]` / `OwnsOne`). Converters as static fields in `Configurations/`. Owned record collections: key `(owner_id, natural key)`; replacing a record with the same key is an UPDATE.
   - **Uniqueness**: Lookup pre-check (`db.<Set>.CodeExistsAsync(...)`) throwing the `Conflict` error **and** a unique index `ux_<table>_<what>` added to the module's `UniqueConstraints` map. Conditional uniqueness (among active rows) → copied column + interceptor + partial index (`ActiveNameInterceptor`, `ux_ingredient_names_active_primary`).
   - Case-insensitive SQL compare: `x.ToLower() == lower` with `#pragma warning disable CA1862` and the reason (no SQL translation for `StringComparison`).
7. **Reacting to events** (projections, follow-ups): `Persistence/Projections/<Name>Handler.cs` or `Features/On<Event>/<Name>Handler.cs`, `public static Task Handle(<Event> e, <X>DbContext db, CancellationToken ct)`. **The class name must end in `Handler`** — Wolverine ignores anything else silently. Rebuild from current state so a duplicate or late delivery is harmless. Its writes are audited as `system:<context>.<handler>` automatically.
8. **Migration**: `dotnet tool restore`, then
   `dotnet ef migrations add <Name> --project dotnet/src/<Project> --startup-project dotnet/src/<Project> --context <X>DbContext --output-dir Persistence/Migrations`.
   Read the generated `Up`: check names (`ux_…`, `ix_…`), nullability, defaults on existing rows. Hand-written SQL (functions, `unaccent` wrappers) goes in with a comment and a matching `Down`. Never edit the snapshot; regenerate a migration that is not committed yet instead of stacking a fix. Until the first production release, regenerate the context's single `Initial<Context>` migration instead of adding one (`dotnet/AGENTS.md`, Persistence).
9. **Module**: a new context gets `<Context>Module.cs` (`Add<Context>Context`, `Migrate…Async` for `DevelopmentDatabase`), its `DbContext` registered with `UseSchemaNpgsql` + `AddCooquoiInterceptors(services, UniqueConstraints)` and singleton options, `OnModelCreating` ending with `AddAggregateAudit()` and `MapIdempotencyCommits()`, and its assembly in `Program.cs` (`Discovery.IncludeAssembly`). Seeds call `SystemActor.Enter("system:<context>.seed")`.
10. **Tests** — `dotnet-test` skill: domain tests per rule, an integration test per endpoint outcome (success, each error code, 401/403, 404, If-Match 428/412 for actions), the event published (or not, on failure).
11. **Refactor** — `refactor` skill (levels 0–2, verification loop).
12. **Self-review** — `dotnet-review` checklist; fix every finding.

## Done when

- `dotnet build dotnet/Cooquoi.slnx`, `dotnet test --solution dotnet/Cooquoi.slnx` and `dotnet format dotnet/Cooquoi.slnx --verify-no-changes` pass — results shown, not assumed (Docker must run for integration tests).
- The new operation, its responses and error codes are in the contract, or the gap is reported.
- The refactor report is in the final message.
- For HTTP behaviour that tests cannot prove (real Keycloak token, Aspire wiring), run it under `aspire start` and report what was checked.
