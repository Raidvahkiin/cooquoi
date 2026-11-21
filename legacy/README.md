# Legacy NestJS backend — reference only

The first Cooquoi backend (NestJS, Drizzle, Postgres), replaced by the C# backend in `dotnet/` ([ADR 0002](../docs/architecture/adr/0002-csharp-backend-with-aspire.md)).

**Frozen.** Nothing here is installed, built, linted, tested or deployed: `legacy/` is outside the pnpm workspace and ignored by Nx (`.nxignore`), TypeScript, ESLint, Biome, Docker and CI. Paths, `catalog:` versions and tsconfig `extends` inside it still point at the old layout, so it does not run as is.

Keep it only to read how a feature worked (Market rules, price comparison, seed data) when porting it to C#. Delete a part once its C# replacement exists; delete the folder when nothing is left to port.

| Folder | Was |
|---|---|
| `apps/cooquoi-api` | `@cooquoi/api` — REST API (Fastify), controllers, seeding, Drizzle migrations |
| `libs/contexts/market` | `@cooquoi/market` — ingredients, products, vendor offers, price comparison |
| `libs/contexts/authority` | Early users/sharing context |
| `libs/core` | `@libs/core` — `Entity`, value objects (`Quantity`, `Unit`, `Currency`), `DomainError` |
| `libs/utils/nestjs` | `@utils/nestjs` — CQRS `Mediator` with middleware |
| `libs/utils/api-contracts` | `@utils/api-contracts` — hand-written models and HTTP client of this API |
| `docker/`, `scripts/` | Local Postgres + API + old webapp compose setup |

## Running it again

Check out the last commit where it was part of the build, in a separate worktree:

```bash
git worktree add ../cooquoi-legacy 7370dd5
```

Then `pnpm install` and `pnpm run dev:api` there.
