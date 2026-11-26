# E02.S02.T01 — Household: `HouseholdCreated` through the outbox and a current-household query

## Goal

The Household context publishes `HouseholdCreated(householdId, creatorLocale)` when `CreatePersonal` runs on first sign-in, through the Wolverine outbox in the same transaction. It also publishes an in-process query that returns the household id of a caller (issuer + subject), or none.

## Read only

- `dotnet/AGENTS.md` "Use cases" (the own-transaction exception), "Persistence" (registration)
- `dotnet/src/Cooquoi.Context.Household/Features/SignIn/UserSignIn.cs:31-58` (CreateOnceAsync, advisory lock, locale at `:58`)
- `dotnet/src/Cooquoi.Context.Household/Domain/Households/HouseholdAggregate.cs:19`, `HouseholdId.cs:3`, `UserProfileAggregate.cs:15-16`
- `dotnet/src/Cooquoi.Context.Household/HouseholdModule.cs:21` vs `dotnet/src/Cooquoi.Context.Foodpedia/FoodpediaModule.cs:29-38`
- The `PublicApi/` and `PublicApi/Events/` convention from the E02.S01.T04 report and `dotnet/AGENTS.md` "Layout"

## Reference

- Registration: switch `HouseholdModule` to `AddDbContextWithWolverineIntegration`, as in `FoodpediaModule.cs:29-38`.
- Event: `CreatePersonal` raises a domain `HouseholdCreatedDomainEvent(HouseholdId, Locale)`. The integration event in `PublicApi/Events/HouseholdCreatedEvent` carries `Guid HouseholdId, string Locale` and follows the S01.T04 mapping-handler pattern.
- Transaction: `UserSignIn` saves in its own transaction under an advisory lock, so Wolverine's transactional middleware does not run there. Use Wolverine's EF Core outbox for an explicit transaction (`IDbContextOutbox<HouseholdDbContext>` and `SaveChangesAndFlushMessagesAsync`) so the event is stored in the same transaction. See Lookups.
- Query: `PublicApi/HouseholdDirectory` (or similar), `Task<Guid?> FindHouseholdIdAsync(string issuer, string subject)`, projecting `UserProfile.DefaultHouseholdId` with `AsNoTracking`.

## Skills

`dotnet-domain`, `dotnet-feature`, `dotnet-test`

## Model

opus (outbox inside an explicit transaction with an advisory lock; no pattern in the repo)

## Lookups

- How does Wolverine 5 publish domain events and messages from an EF Core save in a hand-made transaction? Use context7 (`resolve-library-id` "Wolverine", then `query-docs` "IDbContextOutbox SaveChangesAndFlushMessagesAsync"). Expected: ≤10 lines of API names.

## Out of scope

Foodventory code (T02, T03); the profile and household endpoints' responses

## Subtasks

- [ ] T01.1 Wolverine-integrated `HouseholdDbContext` registration; existing Household tests green
- [ ] T01.2 Domain `HouseholdCreatedDomainEvent` raised by `CreatePersonal`; domain test
- [ ] T01.3 `UserSignIn` saves through the outbox; `PublicApi/Events/HouseholdCreatedEvent` mapping
- [ ] T01.4 `PublicApi` current-household query
- [ ] T01.5 Integration tests: first `POST /v1/me:sign-in` publishes exactly one `HouseholdCreatedEvent` with the profile locale (tracked session); a second call publishes none; the query returns the id, and none for an unknown subject

## Done when

`dotnet build dotnet/Cooquoi.slnx -v q -nologo -clp:ErrorsOnly && dotnet test --project dotnet/tests/Cooquoi.Context.Household.Tests -v q --no-progress` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(household): publish HouseholdCreated and the current household (E02.S02.T01)`), stop.

Status: todo

Report:
