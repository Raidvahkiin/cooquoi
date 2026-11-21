# ADR 0002 — C# backend orchestrated with Aspire

- **Status**: Accepted
- **Date**: 2026-11-20

## Context

The first backend was written in NestJS/TypeScript. In practice it drifted towards CRUD: entities were Drizzle table definitions, and a custom mediator, `Entity`/`ValueObject` base classes and logging pipeline had to be maintained by hand. The goal is a **robust, object-oriented DDD** platform with little boilerplate, and the main developer is most productive in C#.

## Decision

- The backend is written in **C# / .NET**, under `dotnet/`, grouped by **`dotnet/Cooquoi.slnx`** (IDE entry point, `dotnet build` / `dotnet test` target; `global.json` and `Directory.*.props` sit beside it).
- Two orchestrators with distinct roles:
  - **Aspire — runtime orchestration**: the AppHost runs Postgres, Keycloak, the C# API and the Next.js webapp together, with service discovery (URLs and connection strings injected), the Aspire dashboard (OpenTelemetry) and deployment manifests. It is the entry point to **run** the system (`aspire run`).
  - **Nx — task orchestration**: build, lint, test, typecheck, code generation, `nx affected` and caching across all toolchains, locally and in CI. It is the entry point to **build and test** (see [ADR 0001](0001-toolchain-folder-layout.md)). An Nx `dev` target on the AppHost may wrap `aspire run` for a single command.
- Persistence uses **EF Core** with Postgres: owned types and value converters for value objects, backing fields for encapsulated aggregate state, strongly typed ids.
- **One project per bounded context**, organized by vertical slices, plus a shared kernel. No four-layer split per context; boundaries are enforced by architecture tests.
- Integration tests run against a real Postgres (Testcontainers or Aspire testing host).
- The legacy NestJS backend (`apps/cooquoi-api`, `libs/contexts/market`, `libs/utils/nestjs`) is retired once the C# API reaches parity.

## Consequences

- Rich domain models are the natural path (encapsulation, records, structural equality).
- Two toolchains (Node with pnpm, and dotnet — see [ADR 0016](0016-package-manager-and-runtime.md)): Nx runs their tasks, Aspire runs their services.
- End-to-end TypeScript types are lost; the API contract is generated instead (TypeSpec contract-first, see [ADR 0009](0009-api-contract-typespec.md)).
- Modern C# style is required to keep boilerplate low (primary constructors, records, minimal APIs, file-scoped namespaces).
