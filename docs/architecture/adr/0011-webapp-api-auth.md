# ADR 0011 — Webapp ↔ API communication and authentication

- **Status**: Accepted
- **Date**: 2026-11-21

## Context

Users sign in through Keycloak ([ADR 0004](0004-keycloak-identity.md)). The C# API ([ADR 0002](0002-csharp-backend-with-aspire.md)) serves the Next.js webapp now and a Flutter app later ([ADR 0007](0007-mobile-strategy.md)), through one contract ([ADR 0009](0009-api-contract-typespec.md)). We must decide where tokens live and how the webapp reaches the API.

Options considered:

| Option | Summary | Verdict |
|---|---|---|
| **A. Browser calls the API directly** | SPA style: access token held in browser JavaScript, API exposed to the browser with CORS | Rejected: tokens reachable by XSS, token refresh in the browser, CORS to maintain |
| **B. Backend-for-frontend (BFF)** | Next.js server holds tokens in an encrypted httpOnly session cookie and calls the API server-side | **Chosen** for the webapp |
| C. API issues its own session cookies | The API becomes the web auth host | Rejected: couples the API to the webapp; Flutter would need a different scheme anyway |

Libraries considered for the BFF:

| Library | Verdict |
|---|---|
| **`openid-client` + `iron-session`** | **Chosen**: does exactly what is needed, standard OIDC, small explicit code |
| Auth.js | Rejected: built to manage many identity providers itself (Keycloak already does that); uncertain long-term maintenance (merged into the Better Auth project, v5 long in beta) |
| Better Auth | Rejected: designed to be the auth system, with its own user and session tables, duplicating Keycloak and giving Next.js a database |
| `openid-client` + `jose` | Viable; `iron-session` preferred for its ready-made session API and built-in password rotation |
| oauth2-proxy | Rejected: one more service, and token refresh and forwarding are harder |
| `keycloak-js` | Rejected: tokens would live in the browser |

## Decision

### 1. One API, bearer tokens only

- The API is **stateless**: every request carries a Keycloak **access token** (`Authorization: Bearer …`). It never handles cookies or sessions.
- The same API serves every client: the webapp's server side now, Flutter directly later.

### 2. Webapp: Next.js as a backend-for-frontend

Keycloak already handles identity, so the webapp only needs to be **one OIDC client**: no auth framework.

- **`openid-client`** (certified OIDC library) for the protocol: Authorization Code flow with PKCE, code exchange, token refresh, end-session URL. The webapp is registered as a **confidential client**.
- **`iron-session`** for the session: tokens stored in an **encrypted, signed, httpOnly, `Secure`, `SameSite=Lax` cookie**. **The browser never sees a token**; nothing goes in `localStorage`.
  - Password rotation through iron-session's password list (old sessions stay readable after a key change).
  - The session password is a secret in Coolify environment variables.
- All auth code lives in one webapp folder (`src/auth/`): login route, callback route, logout route, and a `getSession()` helper used by Server Components, Server Actions and Route Handlers.
- Access tokens are short-lived (≈5 min); `getSession()` refreshes them server-side with the refresh token when they expire (refresh token rotation enabled in Keycloak).
- **Cookie size**: browsers cap a cookie at ≈4 KB and encryption adds roughly a third, so the session stores **only the access token, the refresh token and the expiry** (no ID token; logout uses `client_id` and the post-logout redirect). Keycloak tokens are kept slim (no unneeded claim mappers; lightweight access tokens if needed). If the session still exceeds the limit, move to a server-side session store.
- **Server Components, Server Actions and Route Handlers** call the API through the generated client (`ts/libs/api-client`), created per request with the bearer token from `getSession()` and the user's `Accept-Language` ([ADR 0012](0012-internationalization.md)).
- Client Components never call the API directly: mutations go through **Server Actions**, client-side data needs (e.g. paging a table) through Server Actions or a thin Route Handler.
- **Server Actions and Route Handlers stay thin**: they forward to the API and contain no business logic, so Flutter can do everything the webapp does.

### 3. Network paths

- Webapp → API uses the **internal address**: Aspire service discovery in development, the internal Docker network in Coolify (e.g. `http://api:8080`). No round trip through the public internet.
- `api.cooquoi.com` stays public for Flutter and other future clients.
- **CORS**: no browser origin is allowed in production, since the webapp never calls the API from the browser. Development tools (API docs UI) may be allowed locally.

### 4. Keycloak clients

| Client | Type | Used by |
|---|---|---|
| `cooquoi-webapp` | Confidential, Authorization Code + PKCE | Next.js server (Auth.js) |
| `cooquoi-mobile` *(later)* | Public, Authorization Code + PKCE | Flutter |
| `cooquoi-api` | Audience | Tokens must carry `aud: cooquoi-api` |

Realm roles (e.g. `catalog-curator`) are included in access tokens.

### 5. API authorization

- ASP.NET **JwtBearer** with Keycloak as authority: validate issuer, audience, signature and lifetime; map realm roles to role claims.
- Two levels of checks:
  - **Role policies** for global capabilities (curating Foodpedia).
  - **Household membership** for owned resources: the household is resolved from the route or the resource, and the caller must be a member with a sufficient role.
- **First login**: the webapp's OIDC callback calls `POST /v1/me:sign-in` before redirecting; the API creates the user's personal household if it does not exist yet (idempotent, ADR 0015 §2). Flutter will do the same.

### 6. Sign-out

- The logout route destroys the iron-session cookie **and** redirects to Keycloak's end-session endpoint, so the Keycloak SSO session ends too.

## Consequences

- Tokens never reach browser JavaScript. CSRF: the session cookie is `SameSite=Lax`, Server Actions have built-in origin checks, and the login flow uses OIDC `state` and PKCE.
- The auth routes are our own code (small, but they must be tested: login, callback, refresh, logout, expired session).
- No CORS configuration to maintain for the webapp.
- The API stays a clean, client-agnostic bearer API, which is what Flutter needs.
- Every webapp data access goes through the Next.js server: slightly more latency than direct browser calls, mitigated by the internal network path.
- Webapp code must not grow business logic in Server Actions; the rule "all behaviour lives in the API" is reviewed in code review.
