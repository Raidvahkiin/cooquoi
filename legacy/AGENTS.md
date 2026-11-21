# Legacy NestJS backend — reference only

Read [README.md](README.md) first. **Do not modify, build or extend this code**; read it only when asked, to port behaviour to the C# backend (`dotnet/`, conventions in `dotnet/AGENTS.md`).

## How it was built (for reading the code)

- **DDD + Vertical Slice + CQRS** with `@nestjs/cqrs`, dispatched through the `Mediator` from `libs/utils/nestjs` (`@EnableLogging()` on commands for the logging middleware).
- Each bounded context is a NestJS library in `libs/contexts/<name>/` exposing a `DynamicModule` via `static register()`. Market details: [libs/contexts/market/AGENTS.md](libs/contexts/market/AGENTS.md).
- One folder per use case: `src/features/<group>/<use-case>/` with `*.command.ts` / `*.query.ts`, `*.handler.ts`, optional `*.dto.ts`, `__tests__/*.spec.ts`.
- Persistence: **Drizzle ORM**; `pgTable` definitions in `src/domain/entities/` are the domain entities (no repository layer).
- Controllers live only in `apps/cooquoi-api/src/controllers/`, importing commands and queries from a context's `index.ts`.
- Dependency tags: `scope:core` → core; `scope:utility` → utility, core; `scope:bounded-context` → core, utility (contexts never import each other); apps → all.
- File suffixes: `.command.ts`, `.query.ts`, `.handler.ts`, `.dto.ts`, `.entity.ts`, `.vo.ts`, `.module.ts`, `.controller.ts`.
- Tests: Vitest; context tests are integration tests against in-memory **PGlite**; `mockPartial<T>()` from `libs/core`; `Test.createTestingModule()`.
- Environment: `DATABASE_URL`, `PORT` (default `3001`), `DATABASE_SEEDING_ENABLED`.
