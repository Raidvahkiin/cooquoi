# Cooquoi

Helps a household decide what to cook and what to buy from what is in the fridge, preferred markets and budget.

## Run everything locally

Requires the .NET 10 SDK, Node (see `.node-version`) with pnpm via corepack, Docker, and the Aspire CLI.

```bash
pnpm install
dotnet tool restore
scripts/dev-certs.sh   # once per machine: trusts the .NET dev certificate
cd aspire/Cooquoi.AppHost && aspire run
```

Aspire starts Postgres, Keycloak, the C# API and the Next.js webapp, and prints the dashboard URL. 

`scripts/dev-certs.sh` trusts the .NET dev certificate and exports it as PEM to `~/.aspnet/dev-certs/https/aspnetcore-dev.pem` (Node trusts it through `NODE_EXTRA_CA_CERTS`). On Linux it also prints the manual browser trust step (NSS `certutil`). Check with `dotnet dev-certs https --check --trust`.

The dev user is `dev@cooquoi.local`; its password is the `dev-user-password` parameter, `cooquoi-dev` by default. Override it with `dotnet user-secrets set Parameters:dev-user-password <value> --project aspire/Cooquoi.AppHost`.

## Checks

```bash
pnpm nx run-many -t lint typecheck test
dotnet build dotnet/Cooquoi.slnx
dotnet test --project dotnet/tests/Cooquoi.Context.Household.Tests
```

## Where things are

See [AGENTS.md](AGENTS.md) for the repository layout and conventions, and [docs/](docs/README.md) for the vision, glossary and architecture decisions. The old NestJS backend is kept for reference only in [legacy/](legacy/README.md).
