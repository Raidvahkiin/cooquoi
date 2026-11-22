# ADR 0018 — HTTPS everywhere in development

- **Status**: Accepted
- **Date**: 2026-11-22

## Context

The S04 run ([findings](../../backlog/E01-spike-walking-skeleton/S04-foodpedia-webapp/findings.md), "Open questions" 2) found development mixing schemes:

- The AppHost serves the webapp on `http://localhost:3000` (`aspire/Cooquoi.AppHost/Resources/Webapp.cs:6-7`) and passes `API_BASE_URL` from the API's `http` endpoint (`:18-28`). The API has an http launch profile only (`dotnet/src/Cooquoi.Api/Properties/launchSettings.json:8`), and so has the AppHost, with `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true` (`aspire/Cooquoi.AppHost/Properties/launchSettings.json:8,14`).
- `Resources/Identity.cs:13-31` builds the issuer and `COOQUOI_WEBAPP_URL` from Keycloak's `http` endpoint, yet Keycloak answered on `https://localhost:<port>` with a self-signed certificate and **HSTS**. After sign-in, browsers and curl upgrade `localhost:3000` to https, where nothing listens.
- The webapp session cookie is `Secure` only in production (`ts/apps/cooquoi-webapp/src/shared/auth/session.ts:32`), so dev never exercises the cookie rules production uses.
- The webapp is a **PWA** ([ADR 0007](0007-mobile-strategy.md)): service workers, camera access and install prompts need a secure context. Dev should behave like production for cookies and these browser APIs.

| Option | Verdict |
|---|---|
| **HTTPS everywhere on the .NET dev certificate** | **Chosen**: same scheme, cookie flags and browser APIs as production; one certificate for every resource |
| HTTP everywhere, HSTS off in the dev realm | Rejected: cookies and PWA APIs behave differently from production; every new HSTS source (Keycloak upgrade, a browser cache) brings the bug back |
| Mixed (only Keycloak on https) | Rejected: the current state, which caused the HSTS upgrade |

## Decision

### 1. Every dev resource on https

- **Aspire**: the AppHost runs its `https` launch profile; `ASPIRE_ALLOW_UNSECURED_TRANSPORT` is removed.
- **API**: an `https` launch profile; the AppHost references the API's `https` endpoint (`API_BASE_URL = api.GetEndpoint("https")`).
- **Webapp**: `next dev` serves https on port 3000 with the .NET dev certificate (exported as PEM); `APP_URL` is `https://localhost:3000`.
- **Keycloak**: served on the dev certificate (not its own self-signed one); the issuer, `OIDC_ISSUER` and `COOQUOI_WEBAPP_URL` come from https endpoints, so the realm's redirect URIs (`${COOQUOI_WEBAPP_URL}` in `aspire/keycloak/realms/cooquoi-realm.json`) are https.
- No http endpoint stays exposed for these resources in dev; Postgres (and Redis, [ADR 0019](0019-webapp-sessions-in-redis.md)) keep their own protocols on the container network.

### 2. Cookies as in production

- The session cookie is `Secure`, `httpOnly`, `SameSite=Lax` in **every** environment; the production-only branch in `session.ts` goes.

### 3. Certificate trust, scripted and documented

- One idempotent script sets up a machine: `dotnet dev-certs https --trust` (on Linux this trusts the certificate for .NET and, where supported, the system store and browsers), exports the certificate as PEM for `next dev` and Keycloak, and prints what is still manual on the user's distribution.
- **Node** trusts the certificate through `NODE_EXTRA_CA_CERTS` pointing at the exported PEM, set by the AppHost for the webapp resource, so server-side `fetch` to the API and Keycloak validates instead of disabling TLS checks.
- `NODE_TLS_REJECT_UNAUTHORIZED=0` and `ignoreHTTPSErrors`-style switches are **not** used outside throwaway scratch scripts.
- The steps (script, manual browser trust on Linux, how to check) are documented in the repository's dev setup.

## Consequences

- Dev, CI integration runs and production share one scheme, one cookie policy and the secure-context browser APIs the PWA needs; the HSTS upgrade bug disappears.
- A new machine needs one script run; on Linux some browsers still need a manual trust step, which the docs list.
- The dev certificate expires (about a year); the script renews it and re-exports the PEM.
- Playwright e2e (later story) can trust the same certificate instead of ignoring TLS errors.
- Production is unchanged: TLS is terminated by Coolify's proxy ([ADR 0006](0006-cicd-github-actions-coolify.md)).
