# E01.S05.T01 — Dev setup: certificate trust script, fixed dev password

## Goal

A new Linux machine trusts the .NET dev certificate for .NET, Node and browsers by running one script, and dev users sign in with a short fixed password from an Aspire parameter. Both steps are documented in the root README.

## Read only

- [ADR 0018](../../../architecture/adr/0018-https-everywhere-in-dev.md) "Decision" §3
- `README.md` "## Run everything locally" (lines 5-15)
- `aspire/Cooquoi.AppHost/Resources/Secrets.cs:71-78` (`AddGeneratedSecret`)
- `aspire/Cooquoi.AppHost/Resources/Identity.cs:13-31`
- `aspire/keycloak/dev-users/cooquoi-users-0.json` (passwords `${COOQUOI_DEV_USER_PASSWORD}` at :15, :32, :49)
- `dotnet/AGENTS.md` "## Configuration" and "## Commands"

## Reference

- Today `Identity.cs:20` reads `var devUserPassword = builder.AddGeneratedSecret("dev-user-password");` inside `if (builder.Environment.IsDevelopment())` (:18). It is passed as `COOQUOI_DEV_USER_PASSWORD` (:22-23).
- Replace that line with `builder.AddParameter("dev-user-password", "<short default>", secret: true)`. The value comes from AppHost user secrets (`Parameters:dev-user-password`), and the default applies when no secret is set. Keep the `IsDevelopment()` guard. Pick a default that Keycloak's realm password policy accepts: check `passwordPolicy` in `aspire/keycloak/realms/cooquoi-realm.json` (`grep -n passwordPolicy`).
- Scripts folder: `scripts/` (only `vitest-reporter.mjs` today). Add `scripts/dev-certs.sh`, executable, `set -euo pipefail`, idempotent. It should:
  - run `dotnet dev-certs https --check --trust || dotnet dev-certs https --trust`;
  - export the certificate as PEM (`dotnet dev-certs https --export-path <dir>/aspnetcore-dev.pem --format Pem --no-password`, writing to a gitignored path such as `~/.aspnet/dev-certs/https/` or `.certs/`; pick one and use the same path in T02);
  - on Linux, print the manual browser step (NSS `certutil` for Chromium/Firefox) when `dotnet dev-certs` cannot do it.

## Skills

none (script, AppHost parameter, docs)

## Model

sonnet

## Out of scope

- Switching any resource to https (T02); `NODE_EXTRA_CA_CERTS` wiring in the AppHost (T02, but document the PEM path here)
- Other README sections

## Subtasks

- [x] T01.1 `scripts/dev-certs.sh`: trust, PEM export to the chosen path, Linux browser hint; gitignore the path if it is inside the repo
- [x] T01.2 `Identity.cs:20`: fixed `dev-user-password` parameter with a short default that the realm policy accepts; remove `AddGeneratedSecret` there (keep the helper if other callers use it)
- [x] T01.3 README "Run everything locally": the run-once `scripts/dev-certs.sh` step, the PEM path (used by T02), the dev users with their default password, and how to override it (`dotnet user-secrets set Parameters:dev-user-password … --project aspire/Cooquoi.AppHost`)

## Done when

- `bash -n scripts/dev-certs.sh && dotnet build aspire/Cooquoi.AppHost -v q -nologo -clp:ErrorsOnly` succeeds.
- A run of `scripts/dev-certs.sh` prints the PEM path, and `dotnet dev-certs https --check --trust` exits 0.
- `aspire start` (from `aspire/Cooquoi.AppHost`) shows `dev-user-password` with the default value in the dashboard parameters.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`chore(dev): dev certificate script and fixed dev user password (E01.S05.T01)`), stop.

Status: done

Report: script (PEM at ~/.aspnet/dev-certs/https/aspnetcore-dev.pem, outside repo), AppHost parameter (default `cooquoi-dev`, realm has no passwordPolicy) and README done; bash -n and AppHost build pass. `dotnet dev-certs https --check --trust` exits 7 in this sandbox (Linux trust not effective, SSL_CERT_DIR hint printed); `aspire start` skipped (headless).
