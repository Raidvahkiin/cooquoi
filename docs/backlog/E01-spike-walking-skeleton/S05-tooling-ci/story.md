# E01.S05 — Tooling and CI (phase 5) and dev environment

## Goal

One CI pipeline across every toolchain (.NET, TypeSpec, TypeScript) that fails on a deliberate violation of each rule, on top of a dev environment that matches production: HTTPS everywhere ([ADR 0018](../../../architecture/adr/0018-https-everywhere-in-dev.md)), a short fixed dev user password, and no tool rewriting committed files.

## Acceptance

- [ ] `aspire run` serves the dashboard, API, webapp and Keycloak on https with the .NET dev certificate; sign-in at `https://localhost:3000` works with no certificate warning and no HSTS upgrade; the session cookie is `Secure` in dev
- [ ] A new Linux machine is set up with one documented script (`dotnet dev-certs https --trust`, PEM export, `NODE_EXTRA_CA_CERTS`)
- [ ] Dev users sign in with a short documented password (Aspire parameter, AppHost user secrets); dev users still exist only in Development
- [ ] `next build`, `pnpm run nx:format` and `pnpm biome check .` leave the working tree clean; no stale `.env.development.local`
- [ ] Nx targets for .NET (build, test, format check) and TypeSpec (compile, generate client)
- [ ] Biome v2 rules (size, complexity, boundaries) + minimal ESLint (Nx boundaries) + knip
- [ ] CI: lint, typecheck, test across all toolchains, Biome, knip, drift check, error catalogue test, mirrored-test check, size report
- [ ] Each rule fails CI on a deliberate violation
- [ ] Spike answers recorded: does `nx affected` span TypeSpec → client → webapp; which Biome rules are stable enough

## Context

- [Spike plan, Phase 5](../../../architecture/spike-walking-skeleton.md) (lines 178-192)
- [ADR 0018](../../../architecture/adr/0018-https-everywhere-in-dev.md) Decision
- [S04 findings](../S04-foodpedia-webapp/findings.md) "Decisions" 2, 4 and "Left for later stories"
- `.github/workflows/ci.yml` (jobs `lint-typecheck-test` :22-49, `contract-drift` :51-94, `build-and-push-images` :96-131)
- `aspire/Cooquoi.AppHost/Resources/` (Webapp.cs, Identity.cs, Secrets.cs)

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-dev-setup-certs-password.md) | Dev setup: certificate trust script, fixed dev password | done | — | yes | s05-t01 |
| [T02](T02-https-everywhere.md) | HTTPS across Aspire, API, Keycloak and webapp | todo | T01 | no | s05-t02 |
| [T03](T03-clean-working-tree.md) | Tools stop rewriting committed files | done | — | yes | s05-t03 |
| [T04](T04-nx-dotnet-targets.md) | Nx targets for .NET; `nx affected` across toolchains | doing | — | yes | s05-t04 |
| [T05](T05-biome-rules-knip.md) | Biome rules (size, complexity, boundaries) and knip | todo | T03 | no | s05-t05 |
| [T06](T06-ci-pipeline-and-findings.md) | One CI pipeline, violation checks, findings | todo | T02, T04, T05 | no | s05-t06 |

T01, T03 and T04 touch disjoint files and can run at the same time. The story review runs after T06 (`backlog-run`).

## Summary
