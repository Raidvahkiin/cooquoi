# ADR 0015 — User profile stored in the backend

- **Status**: Accepted
- **Date**: 2026-11-21

## Context

Keycloak holds identities and credentials ([ADR 0004](0004-keycloak-identity.md)); the API only receives access tokens ([ADR 0011](0011-webapp-api-auth.md)). Several features still need data about users that a token alone cannot provide:

| Need | Example |
|---|---|
| Show **other** members | "Added by Marie", member list — only the *current* user's token carries a name |
| Invite to a household (later) | By email |
| Language preference | UI and AI output in `vi` ([ADR 0012](0012-internationalization.md)) |
| Default household after sign-in | Several households per user, later |
| Account deletion | Remove personal data; audit fields store user ids ([ADR 0014](0014-backend-structure.md) §6e) |

Options considered:

| Option | Verdict |
|---|---|
| Nothing stored locally (token claims only) | Rejected: cannot show other members; anything else needs Keycloak's admin API |
| **Local `UserProfile`, created and refreshed at sign-in** | **Chosen** |
| Keycloak as the only user store (attributes + admin API) | Rejected: couples the domain to Keycloak, slow, hard to test |

## Decision

### 1. `UserProfile` aggregate

Lives in the **Household context** (`Cooquoi.Context.Household`), next to `Household` and `Membership`: they change together (first sign-in creates both a profile and a personal household).

| Field | Notes |
|---|---|
| `UserId` | Our own id (UUID v7). **What every other context stores.** |
| `IdentityRef` | Issuer + Keycloak subject. Unique. Maps a token to a `UserId`. |
| `DisplayName` | Copied from the token at sign-in. |
| `Email` | Copied from the token at sign-in (invitations later). |
| `Locale` | `en` \| `fr` \| `vi` — **owned by Cooquoi**. Initialized from the token's `locale` claim or `Accept-Language`, then changed by the user. |
| `DefaultHouseholdId` | The household opened after sign-in. |
| `LastSignInAt` | Updated at each sign-in. |

- **Our own `UserId`, not the Keycloak subject.** If the identity provider, realm or brokered provider changes, only `IdentityRef` changes — never the ids stored across contexts.
- **Keycloak keeps** credentials, sign-in, email verification, social login and roles. **Cooquoi keeps** only display data and product preferences.
- **Data minimization**: no phone, address, birthday or other attributes.

### 2. Provisioning at sign-in — `POST /v1/me:sign-in`

*Amended 2026-11-23: provisioning moved from `GET /v1/me` to an explicit sign-in command, so every read is side-effect free (page loads no longer write, and no longer race on the profile row).*

Called once per login, by the webapp's OIDC callback (and by Flutter later), with an `Idempotency-Key` like every `POST`:

1. Resolve the token's issuer + subject to a `UserProfile`.
2. **First sign-in**: create the profile **and** a personal `Household` with the user as owner (`Membership` with role `owner`). The household has no stored name; clients display a translated default ("My household") until it is renamed.
3. **Later sign-ins**: refresh `DisplayName`, `Email` and `LastSignInAt` from the token.
4. Answer `204`.

`GET /v1/me` only reads: the profile and the user's households, or `404 common.notFound` when the user never signed in (the webapp then restarts the login).

- **Concurrent first sign-ins** (two tabs): a transaction-scoped advisory lock per identity serializes them; the unique index on `IdentityRef` stays the safety net.
- Name or email changes made in Keycloak reach Cooquoi at the next sign-in — an accepted, small staleness.

### 3. Current user in requests

`Cooquoi.Platform`'s current-user service maps the token's issuer + subject to the `UserId` **once per request** (cached for the request). Use cases, membership checks and audit fields use the `UserId`, never the raw subject.

### 4. Account deletion (outline)

- Deleting an account deletes the `UserProfile` (personal data) and the Keycloak user.
- Audit fields and history keep only a `UserId` that no longer resolves; clients show "deleted user".
- Households where the user was the only member are deleted; shared ones keep their data.
- Details belong to the future privacy ADR.

## Consequences

- Other members' names and preferences are available locally, without calling Keycloak.
- Every context depends on a stable, provider-independent `UserId`.
- One command (`POST /v1/me:sign-in`) covers provisioning and refresh for every client; `GET /v1/me` is a plain read.
- Profile name and email may be slightly stale between sign-ins.
