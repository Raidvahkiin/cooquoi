# Cooquoi

Helps a household decide what to cook and what to buy from what is in the fridge, preferred markets and budget.

## Run everything locally

Requires the .NET 10 SDK, Node (see `.node-version`) with pnpm via corepack, Docker, and the Aspire CLI.

```bash
pnpm install
dotnet tool restore
cd aspire/Cooquoi.AppHost && aspire run
```

Aspire starts Postgres, Keycloak, the C# API and the Next.js webapp, and prints the dashboard URL. The dev user is `dev@cooquoi.local`; its generated password is the `dev-user-password` parameter in the dashboard.

## Checks

```bash
pnpm nx run-many -t lint typecheck test
dotnet build dotnet/Cooquoi.slnx
dotnet test --project dotnet/tests/Cooquoi.Context.Household.Tests
```

## Where things are

See [AGENTS.md](AGENTS.md) for the repository layout and conventions, and [docs/](docs/README.md) for the vision, glossary and architecture decisions. The old NestJS backend is kept for reference only in [legacy/](legacy/README.md).
