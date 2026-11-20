# ADR 0004 — Keycloak as identity broker

- **Status**: Accepted
- **Date**: 2026-11-20

## Context

Every household-owned resource needs an owner from day one, so authentication must exist before the first context. Users should sign in with Google, and possibly a custom OIDC provider later. Roles are needed (e.g. `catalog-curator` for Foodpedia).

## Decision

- **Keycloak** is the identity provider and broker: Google and any custom OIDC provider are configured as identity providers in a Cooquoi realm.
- Keycloak runs under **Aspire** in development.
- The **C# API** validates JWT access tokens; authorization uses Keycloak roles and household membership.
- The **Next.js webapp** signs users in through OIDC (Auth.js with the Keycloak provider).
- The domain stores **no credentials**: users are referenced by their Keycloak subject id. The *Identity & Household* context keeps only `Household` and `Membership`.
- A personal household is created automatically on a user's first login.

## Consequences

- Authentication, social login and role management are off-the-shelf (generic subdomain).
- One more service to run and configure (realm, clients, identity providers); realm configuration should be versioned and imported at startup.
- Keycloak types never leak into the domain (anti-corruption layer at the API boundary).
