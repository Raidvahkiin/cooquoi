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

`scripts/dev-certs.sh` trusts the .NET dev certificate and exports it as PEM to `~/.aspnet/dev-certs/https/aspnetcore-dev.pem` (the webapp `dev` script passes it to `next dev --experimental-https-ca`). On Linux, install `libnss3-tools` first so browsers (NSS) trust it too, and add `export SSL_CERT_DIR="$HOME/.aspnet/dev-certs/trust:/usr/lib/ssl/certs"` to your shell profile for OpenSSL clients. Check with `dotnet dev-certs https --check --trust`.

The dev user is `dev@cooquoi.local`; its password is the `dev-user-password` parameter, `cooquoi-dev` by default. Override it with `dotnet user-secrets set Parameters:dev-user-password <value> --project aspire/Cooquoi.AppHost`.

### Inspect sessions with Redis Insight

The webapp sessions live in the `session-store` Redis. Connect Redis Insight from the host with a plain connection (no TLS): host `localhost`, port `6380` (fixed), username `admin`, password from the `session-store-admin-password` parameter (Aspire dashboard, Parameters; `dotnet user-secrets` does not hold it). Sessions are the `session:*` keys; their values are encrypted.

## Checks

```bash
pnpm nx run-many -t lint typecheck test
dotnet build dotnet/Cooquoi.slnx
dotnet test --project dotnet/tests/Cooquoi.Context.Household.Tests
```

## Where things are

See [AGENTS.md](AGENTS.md) for the repository layout and conventions, and [docs/](docs/README.md) for the vision, glossary and architecture decisions. The old NestJS backend is kept for reference only in [legacy/](legacy/README.md).
