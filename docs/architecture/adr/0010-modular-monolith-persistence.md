# ADR 0010 — Modular monolith and persistence

- **Status**: Accepted
- **Date**: 2026-11-21

## Context

Cooquoi has several bounded contexts ([context map](../../business/context-map.md)) but one developer, one VPS and one Postgres ([ADR 0006](0006-cicd-github-actions-coolify.md)). The API contract uses typed list queries and domain actions ([ADR 0009](0009-api-contract-typespec.md)), which determines how the backend reads and writes data. Contexts must stay isolated so they can evolve, or be extracted, independently.

## Decision

### 1. One deployable, many modules

- A single ASP.NET host, **`Cooquoi.Api`**, hosts every bounded context.
- **One .NET project per bounded context** (`Cooquoi.Foodpedia`, `Cooquoi.Pantry`, `Cooquoi.Household`…), each registering its own endpoints, handlers and `DbContext` through a module entry point.
- Contexts communicate only through **published queries** (in-process calls to the owning context's public API) or **events** via Wolverine ([ADR 0003](0003-wolverine.md)). Never through another context's internals or tables.
- Boundaries are enforced by **architecture tests** (e.g. NetArchTest or ArchUnitNET).

### 2. One database, one schema per context

- **One Postgres database**, with **one schema per context**: `household`, `foodpedia`, `pantry`, … plus `wolverine` for message storage.
- **One `DbContext` per context**, with `HasDefaultSchema("<context>")` and its own migrations history table. A `DbContext` never maps another context's tables.
- **A single connection string** (one database) serves every context; isolation comes from schemas, not separate connections.
- *Alternative rejected*: one Postgres **database** per context on the same server. Strongest isolation, but one connection string per context and Wolverine message storage needed in each database to keep the outbox transactional; more setup and backups for little gain at this scale.
- **No foreign keys across schemas.** A context stores other contexts' ids only (e.g. `IngredientId` in Pantry). Cross-context consistency comes from events (e.g. Foodpedia publishes `IngredientRetired`, Pantry reacts).

### 3. Data access

- **Driver / ORM**: Npgsql with `Npgsql.EntityFrameworkCore.PostgreSQL`, wired through Aspire's Npgsql EF Core integration (connection string, retries, health checks, telemetry).
- **Write side (commands)**:
  - Load the aggregate with EF Core (tracked), call its domain method, save.
  - Handlers use the context's `DbContext` directly — no repository layer for the MVP.
  - Domain events are stored in the **Wolverine outbox in the same transaction** as the aggregate change.
- **Read side (queries)**:
  - Same `DbContext` and tables, with `AsNoTracking`, projecting directly to response DTOs with `.Select(...)`. Aggregates are never loaded only to be read.
  - Typed filters from the contract map to explicit `Where` clauses.
  - **Sorting through a per-read-model whitelist** (`"name"`, `"expiresAt"`, `"-expiresAt"` → expressions). Sort strings are never interpreted dynamically.
  - A shared helper (`ApplySort`, `ToPageAsync`) returns `Page<T>` (count query + `Skip`/`Take`).
  - A heavy screen may later get a **dedicated read table maintained from events**, without changing the contract.
- **Raw SQL / Dapper**: not by default; only for a specific heavy query, hidden behind the same query handler.

### 4. Modelling in EF Core

- Strongly typed ids (`IngredientId`, `StockItemId`) via value converters; ids generated as UUID v7.
- Value objects (`Quantity`, `Money`, `Budget`) as EF Core complex or owned types.
- Encapsulated aggregate state (private setters, backing fields for collections).
- Free-text search (`q`) with Postgres **`pg_trgm` + `unaccent`** (tolerant to typos and accents).

### 5. Migrations

- EF Core migrations **per context**.
- Applied in production by a migration bundle executed before the API starts ([ADR 0006](0006-cicd-github-actions-coolify.md)); applied automatically on startup in local development under Aspire.

## Consequences

- Simple to run and deploy: one API, one database.
- Strong isolation where it matters (schemas, `DbContext`, no cross-schema joins), so a context can be extracted later by moving its schema.
- Queries are finite and known per endpoint, so they can be indexed and tested.
- Without cross-schema foreign keys, referential integrity across contexts relies on events and must be covered by tests.
- One shared database means one shared failure domain and one backup; acceptable at this scale.
