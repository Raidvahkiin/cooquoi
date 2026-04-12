# GitHub Copilot Instructions

## Business Overview

This project is a **food/ingredient management platform** that allows users to manage ingredients, products, recipes, and inventories. The final product is a web app and/or mobile app communicating via REST APIs.

Core capabilities:
- Browse and compare ingredient/product prices from different vendors
- Manage personal inventory and track expiry dates
- Create recipes from ingredients
- Share resources (prices, inventories) between users

## Architecture

The system is built on **DDD (Domain-Driven Design)** + **SOLID** principles with **Vertical Slice Architecture (VSA)**. Each feature is a self-contained vertical slice.

- Each bounded context is a NestJS library module under `libs/contexts/`
- Features are invoked via **NestJS CQRS** (`@nestjs/cqrs`) — commands and queries
- Domain entities **also serve as data models** (no separate ORM entity / repository interface layer)
- Each context exports a NestJS `DynamicModule` via `static register()`

## Monorepo Overview

This is an **Nx monorepo** for the `cooquoi` project, managed with **Bun** as the package manager.

- **Backend**: NestJS (Fastify platform) in `apps/cooquoi-api`
- **Frontend**: React 19 + Next.js with Tailwind CSS v4
- **Testing**: Vitest across all projects
- **Linting/Formatting**: Biome (primary) + ESLint (secondary)
- **Language**: TypeScript ~6 with strict mode

## Project Structure

```
apps/
  cooquoi-api/        # @cooquoi/api — NestJS REST API
libs/
  core/               # @libs/core — shared DDD primitives (Entity, ValueObject, DomainError, etc.)
  contexts/
    market/           # @cooquoi/market — ingredients, products, vendor price offers & comparison
    stock/            # @cooquoi/stock  — inventory management, expiry tracking
    recipes/          # @cooquoi/recipes — recipe creation from ingredients
    authority/        # @cooquoi/authority — user management, resource sharing
  utils/
    react/            # @utils/react — shared React UI component library
packages/             # (future shared packages)
```

## Package Naming & Nx Tags

| Scope | Pattern | Example |
|---|---|---|
| App packages | `@cooquoi/*` | `@cooquoi/api`, `@cooquoi/market` |
| Shared libs | `@libs/*` | `@libs/core` |
| Utility libs | `@utils/*` | `@utils/react` |

Every project is tagged: `type:lib`, `scope:<name>`, `platform:<target>` (`nestjs` \| `react` \| `all`).

## Running Tasks

Always run tasks through Nx, prefixed with `bun`:

```bash
bun nx run <project>:<target>        # single task
bun nx run-many --target=<target>    # all projects
bun nx affected --target=<target>    # only affected
```

Common targets: `build`, `test`, `lint`, `serve`, `storybook`.

## TypeScript Conventions

- `module: "nodenext"`, `moduleResolution: "nodenext"`
- `strict: true`, `noUnusedLocals`, `noImplicitReturns`, `noImplicitOverride`
- `customConditions: ["@org/source"]` — workspace packages resolve to TypeScript source directly (no build step needed during dev)
- Use `workspace:*` for local dependencies in `package.json`
- Each project has three tsconfig files: `tsconfig.json` (base), `tsconfig.lib.json` (build), `tsconfig.spec.json` (tests)

## File Naming Conventions

| Type | Suffix | Example |
|---|---|---|
| Test | `.spec.ts` | `entity.spec.ts` |
| Value Object | `.vo.ts` | `currency.vo.ts` |
| NestJS Module | `.module.ts` | `market.module.ts` |
| Storybook | `.stories.tsx` | `button.stories.tsx` |
| Utility | `.util.ts` | `class-name.util.ts` |

All public APIs are exported through `index.ts` barrel files.

## Bounded Contexts

Each context lives under `libs/contexts/<name>/` and is structured as:

```
src/
  domain/       # Entities, value objects, domain errors for this context
  features/     # One folder per vertical slice (feature)
    <feature>/
      <feature>.command.ts   # or .query.ts
      <feature>.handler.ts
      <feature>.controller.ts (optional, if REST endpoint)
  index.ts      # Public barrel export
  <name>.module.ts
```

### market (`@cooquoi/market`)
CRUD for **ingredients** and **products**. Vendors can attach price offers to products. Provides **price comparison** across vendors.

### stock (`@cooquoi/stock`)
**Inventory management**: add ingredients to inventories, track quantities and **expiry dates**.

### recipes (`@cooquoi/recipes`)
Create and manage **recipes** composed of ingredients with quantities and units.

### authority (`@cooquoi/authority`)
**User management** and **resource sharing**: share product price lists or inventories between users/friends.

## Domain-Driven Design (DDD)

The `@libs/core` library provides DDD building blocks:

- **`Entity<T>`** — abstract base with UUID id, timestamps, and `commitUpdate()` for state mutations. Entities are also used directly as data models (no separate ORM entity layer).
- **`ValueObject`** — interface with `equals()`, `compareTo()`, `toString()`; examples: `Quantity`, `Currency`, `Unit`
- **`DomainError`** — typed error with `domain`, `code`, `message` fields
- **`Enumeration`** — abstract type-safe enum pattern with `equals()`
- **`EntityFilter`** / `GroupedEntityFilter` — AND/OR query building for repositories
- **`GetManyOptions<T>`** / `GetManyResult<T>` — repository query contracts
- **`DatetimeProvider`** / `SystemDatetimeProvider` — injectable datetime abstraction for deterministic tests

Bounded contexts live under `libs/contexts/`. Each exports a NestJS `DynamicModule` via a `static register()` factory method.

## CQRS Pattern

Features within each context use **NestJS CQRS** (`@nestjs/cqrs`):

- Each feature slice has a **Command** or **Query** + a **Handler**
- Commands mutate state; queries read state
- Handlers are registered in the context's module
- Controllers dispatch commands/queries via `CommandBus` / `QueryBus`

## Testing Patterns

- Use **Vitest** with `globals: true`
- Use `mockPartial<T>()` from `@libs/core/test-utils` for partial Vitest mocks (type-safe)
- Use `SystemDatetimeProvider` or a stub when testing time-dependent logic
- NestJS tests use `@nestjs/testing` with `Test.createTestingModule()`

## Code Style

- **Biome**: 2-space indent, single quotes, `organizeImports` enabled
- No `useImportType` enforcement (turned off in Biome config)
- Prefer type imports (`import type`) where applicable
- Use `clsx` + `tailwind-merge` for conditional class names in React components
- Use `class-variance-authority` (CVA) for component variant definitions
