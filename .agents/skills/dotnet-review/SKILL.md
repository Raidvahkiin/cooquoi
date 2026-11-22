---
name: dotnet-review
description: Review a change to the Cooquoi C# backend against its conventions — layers and dependency rules, domain kinds and rule placement, thin Wolverine endpoints, HTTP semantics (If-Match, ETag, 201/204, idempotency), persistence and migrations, events and handler naming, error codes in the contract, tests and the refactor report. Use when reviewing backend code written by a person or an agent, and as a self-check at the end of dotnet-feature.
---

# .NET review

Conventions: `dotnet/AGENTS.md`. Report findings as a list: **file:line — problem — expected fix**, most severe first. Do not approve with open findings. Check behaviour by running things, not by reading only.

## Checklist

### Layers and boundaries
- [ ] `Domain/` references only `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`; no EF, Wolverine, ASP.NET, Platform.
- [ ] No reference to another context (later: only `Integration/` → its `PublicApi`).
- [ ] No use case calls another; shared logic is in the domain or `Persistence/Lookups/`.
- [ ] `Raidvahkiin.*` stays product-neutral; new building-block code is used by two contexts the same way.

### Domain
- [ ] Names match kinds: `…Aggregate`, `…Entity`, plain immutable value objects; DTOs use plain glossary names.
- [ ] Each rule is in the right place (value object / aggregate guard / pre-check + constraint / policy / validator) and carries its id (comment + test name).
- [ ] Methods check every rule before changing state; every behaviour raises an event.
- [ ] Errors: `<context>.<kebab>` codes, right `ErrorKind`, no reused codes; every new code exists in the contract (or is reported as missing).

### Endpoints and HTTP
- [ ] Endpoint = handler: `LoadAsync` (throws `CommonError.NotFound`) → one domain call. No business logic, no `SaveChanges`, no `try/catch`, no hand-built error responses.
- [ ] `OperationId`, route and status codes match the contract; curator-only operations have `[Authorize(Policy = CooquoiPolicies.Curator)]`, others `[Authorize]`.
- [ ] Replace-style actions call `IfMatch.Require` and answer 204 with `SetETagAfterSave`; intent actions do not require `If-Match`. Reads return `version` and set `ETag`. Creates return 201 + `Location` + `{ id }`.
- [ ] Validators check shape only and use `validation.*` codes; query records are constructor-bound for `[AsParameters]`.
- [ ] Queries: `AsNoTracking`, projection (no aggregate loaded), explicit filters, `SortMap` whitelist with a tie-breaker, `ToPageAsync`, page size capped.

### Persistence
- [ ] Mapping fits the need (child table if queried/constrained, jsonb if read whole, `text[]` for code sets); `Version` is a concurrency token; `DomainEvents` ignored; no audit fields in domain classes.
- [ ] Each cross-aggregate rule has a pre-check **and** a named unique index mapped in `UniqueConstraints`.
- [ ] The migration was generated (not hand-written), matches the model, has a sensible `Down`, and hand-written SQL is commented. The snapshot is untouched by hand.
- [ ] New `DbContext`: `UseSchemaNpgsql`, `AddCooquoiInterceptors`, singleton options, `AddAggregateAudit()`, `MapIdempotencyCommits()`.

### Events and handlers
- [ ] Handler classes end in `Handler` / `Consumer` (else Wolverine ignores them — the architecture test catches it).
- [ ] Handlers are idempotent (rebuild from state, or record the event id) and tolerate late delivery.
- [ ] Events carry ids and primitives; integration events (other contexts) are separate `<Fact>Event` types in `PublicApi/Events/` with a versioned identity.

### Clean code
- [ ] Sizes: aggregate ≤ 200 lines, value object ≤ 100, use-case files ≤ 80, methods ≤ 30; ≤ 2 parameters on domain/helper methods (else a record).
- [ ] One top-level type per file (a use-case file may hold its request, nested inputs and validator); file-scoped namespaces; no commented-out or dead code; suppressions (`#pragma`) carry their reason.
- [ ] Comments say why, not what.

### Tests
- [ ] Domain test per rule and behaviour (rule ids in names); integration test per endpoint outcome (success, each error, 401/403, 404, 428/412).
- [ ] Event assertions filter by the test's own ids (tests run in parallel).
- [ ] No production change for testability, no mocks of our own code, no weakened assertions; new rules were seen failing once.

### Refactor report and checks
- [ ] A refactor report exists (`refactor` skill), skipped smells have an allowed reason.
- [ ] `dotnet build dotnet/Cooquoi.slnx`, `dotnet test --solution dotnet/Cooquoi.slnx`, `dotnet format dotnet/Cooquoi.slnx --verify-no-changes` pass — output shown, not assumed.
