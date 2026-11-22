# E01.S07.T01 — Redis as an Aspire resource

## Goal

`aspire run` starts a Redis 8 container for the webapp. It is capped in memory, has no persistence, and its connection string reaches the webapp as `REDIS_URL`.

## Read only

- [ADR 0019](../../../architecture/adr/0019-webapp-sessions-in-redis.md) "Decision" §2
- `aspire/Cooquoi.AppHost/Program.cs:5-10`
- `aspire/Cooquoi.AppHost/Resources/Database.cs:8-12`, `Resources/Webapp.cs` (whole, ~30 lines)
- `dotnet/Directory.Packages.props:5-11` (Aspire package versions; the AppHost imports it through `aspire/Directory.Packages.props:2`)
- `aspire/Cooquoi.AppHost/Cooquoi.AppHost.csproj:7-11`

## Reference

- Resource pattern: `Database.cs:8-12`, an internal static class with an `AddCooquoiDatabase()` extension, `builder.AddPostgres("postgres")…`. Add `Resources/Cache.cs` with `AddCooquoiCache()` returning `IResourceBuilder<RedisResource>`. Wire it in `Program.cs` and pass it to `AddCooquoiWebapp(api, keycloak, cache, secret)`.
- Package: `Aspire.Hosting.Redis` at the same version as `Aspire.Hosting.PostgreSQL` (17.0.0), added to `Directory.Packages.props` and the AppHost csproj.
- Configuration:
  - Redis 8 image tag pinned.
  - Arguments `--maxmemory 64mb --maxmemory-policy volatile-ttl --save "" --appendonly no`.
  - No `WithDataVolume`.
  - Redis Insight / Commander off by default.
- Webapp env: `REDIS_URL`, a `redis://` URL built from the resource endpoint and password with a `ReferenceExpression` (follow the `Identity.IssuerUrl` style at `Identity.cs:30-31`). `.WaitFor(cache)`.

## Skills

none (AppHost)

## Model

haiku (resource pattern given)

## Lookups

- The exact `Aspire.Hosting.Redis` API in 17.x for args, image tag and password, and how to get a `redis://` URL (`GetEndpoint("tcp")`, `PasswordParameter`). Use `microsoft_docs_search` with "Aspire Redis hosting integration". Expected: ≤10 lines of method names.

## Out of scope

- Webapp code (T02, T03); OCR queue usage (future ADR)

## Subtasks

- [ ] T01.1 Package version and csproj reference
- [ ] T01.2 `Resources/Cache.cs` with args and no volume; `Program.cs` wiring
- [ ] T01.3 `Webapp.cs`: `REDIS_URL` and `WaitFor`

## Done when

- `dotnet build aspire/Cooquoi.AppHost -v q -nologo -clp:ErrorsOnly` succeeds.
- Under `aspire start`, the dashboard shows `cache` healthy, and `docker exec <redis container> redis-cli -a <pw> CONFIG GET maxmemory*` shows 64 MB and `volatile-ttl`.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(aspire): Redis resource for webapp sessions (E01.S07.T01)`), stop.

Status: todo

Report:
