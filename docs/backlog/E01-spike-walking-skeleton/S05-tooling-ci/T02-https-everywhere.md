# E01.S05.T02 — HTTPS across Aspire, API, Keycloak and webapp

## Goal

Under `aspire run`, the dashboard, API, Keycloak and webapp serve https on the .NET dev certificate. The webapp at `https://localhost:3000` signs in through an https Keycloak issuer, and Node trusts the certificate. The session cookie is `Secure` in every environment ([ADR 0018](../../../architecture/adr/0018-https-everywhere-in-dev.md)).

## Read only

- [ADR 0018](../../../architecture/adr/0018-https-everywhere-in-dev.md) "Decision"
- `aspire/Cooquoi.AppHost/Resources/Webapp.cs` (whole, ~30 lines), `Resources/Identity.cs:13-34`, `Resources/Api.cs:9-15`
- `aspire/Cooquoi.AppHost/Properties/launchSettings.json`, `dotnet/src/Cooquoi.Api/Properties/launchSettings.json`
- `ts/apps/cooquoi-webapp/src/shared/auth/session.ts:26-37`
- `ts/apps/cooquoi-webapp/project.json` (the `dev` target) and `package.json` scripts
- README "Run everything locally", as left by T01 (PEM path)

## Reference

The current lines to change:

- `Webapp.cs:7` is `Url = $"http://localhost:{Port}"`, `:20` is `.WithEndpoint("http", endpoint => endpoint.Port = Port)`, and `:24` is `API_BASE_URL = api.GetEndpoint("http")`.
- `Identity.cs:30-31` is `IssuerUrl = ReferenceExpression.Create($"{keycloak.GetEndpoint("http")}/realms/{Realm}")`. `:16` sets `COOQUOI_WEBAPP_URL` from `Webapp.Url`. The realm redirect URIs follow `${COOQUOI_WEBAPP_URL}` in `aspire/keycloak/realms/cooquoi-realm.json`.
- AppHost `launchSettings.json`:
  - It has a single `"http"` profile (:4).
  - Its URLs are `http://localhost:15149` (:8), OTLP `http://localhost:19066` (:12) and resource service `http://localhost:20101` (:13).
  - `ASPIRE_ALLOW_UNSECURED_TRANSPORT` is set to `true` (:14).
- API `launchSettings.json` has a single `"http"` profile on `http://localhost:5281` (:8).
- `session.ts:32` is `secure: process.env.NODE_ENV === 'production'`.

## Skills

none (AppHost and config); `webapp-test` if a session test asserts the cookie flags

## Model

opus (cross-cutting across Aspire, Keycloak, Node and the browser; no pattern to copy)

## Lookups

- Aspire 13 (package versions in `dotnet/Directory.Packages.props:5-11`) and Keycloak `17.0.0-preview`: how to put the Keycloak container on the dev certificate (`WithHttpsDeveloperCertificate()` or `KC_HTTPS_CERTIFICATE_FILE` with a bind mount), and how to give a Next.js resource an https endpoint (`WithHttpsEndpoint`). Answer with the API names from the Microsoft Learn docs (`microsoft_docs_search`). Expected: ≤10 lines.
- `next dev` https flags for Next.js 16 (`--experimental-https --experimental-https-key --experimental-https-cert`): check `ts/apps/cooquoi-webapp/node_modules/next/dist/docs/` or `next dev --help`.

## Out of scope

- The trust script and README dev setup (T01); Redis (E01.S07); Playwright
- Production TLS (Coolify proxy, unchanged)

## Subtasks

- [x] T02.1 AppHost `launchSettings.json`: an `https` profile with https dashboard, OTLP and resource-service URLs; remove `ASPIRE_ALLOW_UNSECURED_TRANSPORT`. API `launchSettings.json`: an `https` profile.
- [x] T02.2 `Api.cs`: the `https` endpoint is the one used. `Webapp.cs`: `https://localhost:3000`, an https endpoint, `API_BASE_URL` from `GetEndpoint("https")`, and `NODE_EXTRA_CA_CERTS` set to the PEM path from T01. The webapp `dev` command serves https with that certificate and key.
- [x] T02.3 `Identity.cs`: Keycloak on the dev certificate, `IssuerUrl` and `COOQUOI_WEBAPP_URL` on https. Check that the realm redirect URIs resolve to https.
- [x] T02.4 `session.ts:32`: `secure: true` in every environment. Adjust any test that asserts the old flag.
- [x] T02.5 Run the system: `aspire start`, then sign in as `dev@cooquoi.local` at `https://localhost:3000`. Check there is no certificate warning, no http→https upgrade and a `Secure` session cookie (`curl -sv` against `/auth/login` with the PEM via `--cacert`, then the browser pane). Update the README ports line if it changed.

## Done when

- `dotnet build aspire/Cooquoi.AppHost -v q -nologo -clp:ErrorsOnly` and `pnpm nx run-many -t typecheck test -p @cooquoi/webapp 2>&1 | tail -10` are green.
- Under `aspire start`, every resource URL in the dashboard is https, and the sign-in round trip in T02.5 succeeds.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(dev): HTTPS everywhere in development (E01.S05.T02)`), stop.

Status: done

Report:
- Changed: AppHost and API `https` launch profiles (dashboard 17149, OTLP 21066, resources 22101, API 7281, no unsecured transport); Keycloak `WithHttpsDeveloperCertificate()` (experimental, pragma); webapp endpoint scheme https; `API_BASE_URL` from the API `https` endpoint; `next dev --experimental-https` with the T01 key, cert and `--experimental-https-ca`; session cookie `secure: true`.
- Verified: build and webapp typecheck/test green; under `aspire start` every resource URL is https; curl `--cacert` round trip as `dev@cooquoi.local`: `/auth/login` → https Keycloak → `https://localhost:3000/auth/callback` → `/account` 200, `Secure` cookie, no HSTS. Browser pane: `ERR_CERT_AUTHORITY_INVALID` only because NSS trust (`certutil`) is not installed.
- Surprising: Keycloak and Next.js keep their endpoint named `http` with an https scheme (`GetEndpoint("https")` throws), so `IssuerUrl` still reads `http`. Aspire overwrites `NODE_EXTRA_CA_CERTS` with its own bundle and `next dev` drops it for its server unless `--experimental-https-ca` is passed: that flag carries the PEM, not an AppHost env var.
- Left: `oidc.ts` `allowInsecureRequests` branch for http issuers is now unused in dev (not in scope); README has no ports line, updated the one in `spike-walking-skeleton.md`.
