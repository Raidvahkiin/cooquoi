# ADR 0009 — API contract: TypeSpec contract-first

- **Status**: Accepted
- **Date**: 2026-11-21

## Context

The API is consumed by the Next.js webapp now and by a Flutter app later ([ADR 0007](0007-mobile-strategy.md)), and implemented in C# ([ADR 0002](0002-csharp-backend-with-aspire.md)). Clients must not depend on hand-written types, and the contract must outlive any one implementation.

The domain is **task-based**: consuming stock, confirming a conversion or restocking from a receipt are business actions with invariants, not field updates. A pure CRUD contract (generic `PATCH` on every resource) would move those rules into clients.

Lists still need a uniform way to filter, sort and paginate — the useful idea behind the DataUI CRUD convention used at the developer's company (a NestJS/TypeORM library, not applicable to .NET).

Query options considered:

| Option | Verdict |
|---|---|
| **Typed list query** — each list declares its allowed filters, plus standard sort and paging | **Chosen** |
| Gridify / Sieve — DataUI-like filter string mapped to EF Core | Rejected as default: untyped in the contract, open-ended queries |
| OData | Rejected: poor fit with TypeSpec, exposes the EF model |
| GraphQL (Hot Chocolate) | Rejected: second schema competing with TypeSpec, per-field authorization, N+1, learning cost; screens are known and simple |

## Decision

### 1. TypeSpec is the source of truth

- The contract lives in `contracts/`, written in **TypeSpec**, compiled to **OpenAPI 3** (`contracts/generated/openapi.yaml`).
- Clients are **generated** from it; the C# server is **verified** against it.

### 2. Uniform reads, domain-specific writes

- **Reads** follow one convention everywhere: `list` (typed `ListQuery<TFilter>` → `Page<T>`) and `read` (by id).
- **Writes** depend on the aggregate:
  - CRUD-shaped data (Foodpedia curation, renaming a pantry): standard `create` / `update` / `delete`.
  - Behaviour-rich aggregates (stock items, households, meal plans): **named actions** using the custom-method style `POST /resource/{id}:verb` (e.g. `:consume`, `:discard`, `:restock`, `/conversion:confirm`). No generic `PATCH` on them.
- Conventions are written **once as TypeSpec templates** (`List`, `Read`, `Create`, `Update`, `Delete`, `Action`) and reused by every context.

### 3. List query convention

```tsp
model ListQuery<TFilter> {
  ...TFilter;                                  // typed, per-resource filters
  @query q?: string;                           // free-text search
  @query sort?: string[];                      // "name", "-expiresAt" (minus = descending)
  @query page?: int32 = 1;
  @query @maxValue(100) pageSize?: int32 = 20; // server-enforced maximum
}

model Page<T> {
  items: T[];
  page: int32;
  pageSize: int32;
  totalCount: int64;
}
```

- Filters are **explicit and typed per resource** (e.g. `StockItemFilter { ingredientId?, expiringBefore?, status? }`).
- `sort` accepts only the fields each resource whitelists.
- **Offset paging** by default; cursor paging may be added later for infinite scroll.
- **Escape hatch**: an endpoint that genuinely needs ad-hoc filtering (e.g. a curator grid) may accept a Gridify `filter` string over an explicit field whitelist. This is an exception, recorded in the endpoint's docs.

### 4. Conventions

- Errors: **`ProblemDetails`** (RFC 9457); validation errors as `ValidationProblem` with field errors.
- Versioning: URL prefix **`/v1`**, managed with `@typespec/versioning`.
- Ids: opaque strings (UUID v7 server-side).
- Auth: OAuth2 / OIDC bearer tokens from Keycloak ([ADR 0004](0004-keycloak-identity.md)), declared in the contract.
- Shared value types (`Id`, `Money`, `Quantity`, `Unit`, timestamps) defined once in `common/`.

### 5. Layout

```
contracts/
├─ main.tsp              # @service, servers, auth, versions
├─ tspconfig.yaml        # openapi3 emitter → generated/openapi.yaml
├─ common/               # operations.tsp, paging.tsp, errors.tsp, types.tsp
├─ contexts/
│  ├─ household/         # models.tsp, routes.tsp
│  ├─ foodpedia/
│  └─ foodventory/
└─ generated/openapi.yaml
```

One TypeSpec namespace per bounded context, emitted as an OpenAPI tag, which becomes a client group.

### 6. Generation and verification

| Target | Tool |
|---|---|
| TypeScript client | **Hey API** (`@hey-api/openapi-ts`) — fetch-based SDK, usable in Next.js Server Components — into `ts/libs/api-client`, replacing the hand-written `@utils/api-contracts` |
| Dart client (later) | openapi-generator (`dart-dio`) into `mobile/` |
| C# server | **Hand-written**, then verified: CI compares the OpenAPI document produced by ASP.NET with the TypeSpec one (e.g. `oasdiff`); drift or breaking changes fail the build |

All steps run as Nx targets, so a contract change rebuilds affected clients and checks.

## Consequences

- Every client gets the same typed API, including future Flutter.
- Business rules stay server-side; clients express intent, not state changes.
- Each list endpoint maps to a finite, indexable set of queries (see [ADR 0010](0010-modular-monolith-persistence.md)).
- Endpoints are written twice (TypeSpec and C#); the CI drift check keeps them aligned.
- No generic query language: a new filter means a contract change — deliberate, and visible in review.
