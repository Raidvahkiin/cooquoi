# ADR 0006 — CI/CD: GitHub Actions + Coolify on a VPS

- **Status**: Accepted
- **Date**: 2026-11-20

## Context

Cooquoi is developed by a single developer without a dedicated DevOps role. A VPS with **Coolify** (self-hosted PaaS) is already available, and the domain **`cooquoi.com`** is owned. CI already runs on GitHub Actions and pushes Docker images to GHCR on `v*` tags. Dokploy was considered as an alternative to Coolify.

## Decision

### CI — GitHub Actions

- On every push and pull request: `lint`, `typecheck`, `test` through Nx across all toolchains.
- On `v*` tags: build Docker images (API, webapp) and push them to **GHCR** (`ghcr.io/raidvahkiin/cooquoi/<app>`), tagged with the semver version, `sha` and `latest`.

### CD — Coolify

- **Images are built in CI, never on the VPS** — building on the VPS would compete with the running app for CPU and memory.
- After pushing images, the workflow calls the **Coolify deploy webhook** (token stored as a GitHub secret). Coolify pulls the new image tag and restarts the service.
- **Release = push a `v*` tag.** Nothing reaches production on a plain push to `main`.
- **Rollback**: redeploy the previous image tag from Coolify.
- **Keep Coolify** rather than switching to Dokploy: it is already installed, more mature, and offers everything needed (compose deployments, one-click Postgres/Keycloak, scheduled backups, automatic HTTPS).

### Runtime topology

| Service | Hosted as | Domain |
|---|---|---|
| Next.js webapp | Coolify app (GHCR image) | `cooquoi.com` (`www.cooquoi.com` redirects) |
| C# API | Coolify app (GHCR image) | `api.cooquoi.com` |
| Keycloak | Coolify service, production mode, own database | `auth.cooquoi.com` |
| Postgres | Coolify service, not exposed publicly | — |

- **DNS**: `A` records for `cooquoi.com`, `www`, `api` and `auth` point to the VPS IP. TLS certificates are issued automatically by Coolify.

### Operations defaults (MVP)

| Topic | Default |
|---|---|
| Environments | **Production only**; add staging when there are real users |
| Secrets | Coolify environment variables, never committed. GitHub holds only the deploy-webhook token and GHCR access |
| Database backups | Coolify scheduled Postgres backups. **Target (later)**: a home **Raspberry Pi** running an S3-compatible store (e.g. MinIO or Garage), reached from the VPS over a private tunnel (e.g. Tailscale/WireGuard) — not exposed publicly. **Until then**: local scheduled backups plus VPS provider snapshots if available, and a periodic manual download of a dump. Must be in place before real users store data |
| Migrations | EF Core migration bundle executed before the API starts |
| Keycloak realm | Realm configuration versioned in git and imported at startup |
| VPS sizing | **4 vCPU / 8 GB RAM / 75 GB local disk**. The stack needs ~3–4 GB, leaving headroom for a future staging environment. Disk is the tighter resource: enable Coolify's Docker image cleanup, and keep backups offsite (local disk is not a backup) |

### To evaluate — Aspire publish to Docker Compose

The Aspire AppHost already describes the whole topology. `aspire publish` (Docker Compose integration) can generate a compose file that Coolify deploys directly, making the AppHost the single source of truth for local and production topology. The generated file usually needs production adjustments (secrets, Keycloak production mode, volumes), so the expected workflow is *generate once, commit, adjust* rather than regenerate on every release. To be evaluated once the AppHost exists; until then, services are configured individually in Coolify.

## Consequences

- Simple, low-maintenance pipeline suited to a solo developer: tag to release, one click to roll back.
- The VPS is a single point of failure; offsite backups are mandatory, and restore must be tested at least once.
- Coolify is a dependency of the deployment process; moving away from it later means reconfiguring services, but images and compose files remain portable.
