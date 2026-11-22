# E01.S05 — Tooling and CI (phase 5) and dev environment

## Goal

One CI pipeline across every toolchain (.NET, TypeSpec, TypeScript) that fails on a deliberate violation of each rule, on top of a dev environment that matches production: HTTPS everywhere ([ADR 0018](../../../architecture/adr/0018-https-everywhere-in-dev.md)), a short fixed dev user password, and no tool rewriting committed files.

## Acceptance

- [x] `aspire run` serves the dashboard, API, webapp and Keycloak on https with the .NET dev certificate; sign-in at `https://localhost:3000` works with no certificate warning and no HSTS upgrade; the session cookie is `Secure` in dev — *curl round trip verified (https, `Secure` cookie, no HSTS); NSS and OpenSSL trust done after installing `libnss3-tools`*
- [x] A new Linux machine is set up with one documented script (`dotnet dev-certs https --trust`, PEM export, `NODE_EXTRA_CA_CERTS`)
- [x] Dev users sign in with a short documented password (Aspire parameter, AppHost user secrets); dev users still exist only in Development
- [x] `next build`, `pnpm run nx:format` and `pnpm biome check .` leave the working tree clean; no stale `.env.development.local`
- [x] Nx targets for .NET (build, test, format check) and TypeSpec (compile, generate client)
- [x] Biome v2 rules (size, complexity, boundaries) + minimal ESLint (Nx boundaries) + knip
- [x] CI: lint, typecheck, test across all toolchains, Biome, knip, drift check, error catalogue test, mirrored-test check, size report
- [x] Each rule fails CI on a deliberate violation — *verified locally through the same Nx targets ([findings](findings.md)); first GitHub run pending*
- [x] Spike answers recorded: does `nx affected` span TypeSpec → client → webapp; which Biome rules are stable enough

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
| [T02](T02-https-everywhere.md) | HTTPS across Aspire, API, Keycloak and webapp | done | T01 | no | s05-t02 |
| [T03](T03-clean-working-tree.md) | Tools stop rewriting committed files | done | — | yes | s05-t03 |
| [T04](T04-nx-dotnet-targets.md) | Nx targets for .NET; `nx affected` across toolchains | done | — | yes | s05-t04 |
| [T05](T05-biome-rules-knip.md) | Biome rules (size, complexity, boundaries) and knip | done | T03 | no | s05-t05 |
| [T06](T06-ci-pipeline-and-findings.md) | One CI pipeline, violation checks, findings | done | T02, T04, T05 | no | s05-t06 |

T01, T03 and T04 touch disjoint files and can run at the same time. The story review runs after T06 (`backlog-run`).

## Summary

> Revised by [E01.S08](../S08-build-tool-split/story.md): `@cooquoi/dotnet`, `verify` and `contract-drift` below are historical; CI is now jobs `ts` and `dotnet`.

All six tasks done and pushed (5e9fafa, f96143a, bd4a82d, 8ab686d, 4e327f5, 02f6661), plus review fixes. HTTPS everywhere in dev on the .NET dev certificate, fixed dev password `cooquoi-dev`, a clean working tree after build and format, `@cooquoi/dotnet` Nx targets with the contract chain, Biome size/complexity/boundary rules (all stable) and knip, and one CI pipeline; each of 10 rules fails its Nx target on a deliberate violation ([findings](findings.md)).
`nx affected` spans TypeSpec → api-client → webapp; C# changes do not reach the contract, so `contract-drift` is its own job and must be a required check.
Left: the first real GitHub CI run (`nx-set-shas`), branch protection for `verify` and `contract-drift`.
