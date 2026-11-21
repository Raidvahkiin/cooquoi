---
name: dotnet-domain
description: Design or change the domain of a Cooquoi bounded context in C# — aggregates, entities, value objects, strongly typed ids, domain events and error definitions — with rule ids from the domain docs, check-first-then-change methods and naming by kind. Use when adding or changing business rules, before writing the use case (dotnet-feature), and whenever a rule seems to belong in a handler.
---

# .NET domain

Conventions: `dotnet/AGENTS.md` (Domain, Errors). Reference: `dotnet/src/Cooquoi.Context.Foodpedia/Domain/Ingredients/`. Read the context's `docs/contexts/<context>/domain-model.md` first, only the sections for the aggregates you touch (`grep -n '^#'` it, then read those sections): its rule ids (F1, F2…) and terms are the source of truth.

## Steps

1. **List the rules** the change touches, with their ids. A rule without an id in the doc: add it to the doc first (or ask), then code it.
2. **Place each rule** — the first row that fits:

   | The rule needs… | Goes in |
   |---|---|
   | One value | The value object's constructor |
   | This aggregate's state | A guard in the aggregate method |
   | Other aggregates of this context | Use-case pre-check (`Persistence/Lookups/`) **and** a DB constraint (dotnet-feature) |
   | Another context | Eventual consistency through events |
   | Who the caller is | Authorization policy on the endpoint |
   | Request shape (required, length, one-of) | FluentValidation in the use case |

3. **Types**, one top-level type per file in `Domain/<Aggregate>/`:
   - Id: `public readonly record struct <X>Id(Guid Value) { public static <X>Id New() => new(Guid.CreateVersion7()); }`
   - Value object: `sealed record` with an explicit constructor that guards with `Ensure.That(...)` and exposes get-only properties. Collections with rules are their own value object (see `ConversionTable`: immutable, returns a new instance, custom `Equals`).
   - Aggregate: `public sealed class <X>Aggregate : AggregateRoot<<X>Id>`, private parameterless ctor, private setters, backing `List<>` fields exposed as `IReadOnlyList<>`, `public static <X>Aggregate Create(New<X> request)` taking a parameter record when it needs more than 2 values.
   - Entity (identity inside the aggregate only): `<X>Entity : Entity<TId>`.
   - Enums for closed sets (`IngredientStatus`); a code with rules is a value object (`IngredientCode`, `TagCode`).
4. **Methods: check every rule first, then change state.** Build new values (which validate themselves) before assigning anything; a failure must leave the aggregate untouched. Put the rule id in a comment on the guard: `// F7`.
5. **Raise an event in every behaviour** (and in `Create`): `sealed record <PastTenseFact>(<X>Id <X>Id, …) : IDomainEvent`, one file each. Prefer ids and primitives: events are serialized to the outbox (JSON). Every change raising an event is also what lets the version token and projections follow.
6. **Errors**: add to `static class <X>Error` — `new DomainError("<context>.<kebab-code>", ErrorKind.<Kind>, "English template with {params}.")`. Kinds: `BusinessRule` (422) for broken rules, `Conflict` (409) for uniqueness, `NotFound` via `CommonError.NotFound`. Pass params as pairs: `Ensure.That(ok, IngredientError.ConversionNotNeeded, ("unit", unit.Code))`. Never reuse a code for another meaning. **Every new code goes into the contract** (`contracts/contexts/<context>/`) — say so in the final message if you cannot do it now.
7. **Shared concepts** used by several contexts (`Locale`, `Unit`) live in `Cooquoi.SharedKernel`; reusable product-neutral patterns in `Raidvahkiin.*` only once two contexts use them the same way.
8. **Tests**: run `dotnet-test` for every aggregate behaviour, value object and rule (test names carry the rule id: `F7_a_unit_that_converts_universally_needs_no_conversion`).

## Do not

- Inject services, `DbContext`, `TimeProvider` or `HttpContext` into aggregates — pass values (a `DateTimeOffset now`) or a parameter record.
- Add setters (other than private on aggregates/entities) or mutable collections to value objects — the architecture test fails.
- Name an aggregate without `Aggregate` or a non-aggregate with it.
- Validate request shape in the domain *instead of* the validator, or business rules in the validator.
- Return a generic `Result<T>`: broken rules throw `DomainException` (the transaction rolls back and no event is published).

## Done when

- `dotnet build dotnet/Cooquoi.slnx` is clean (warnings are errors) and the architecture tests pass.
- Each rule touched has a guard with its id and a test with its id; each new error code is listed for the contract.
