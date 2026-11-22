# E01.S05 — Tooling and CI (phase 5)

## Goal

One CI pipeline across every toolchain (.NET, TypeSpec, TypeScript) that fails on a deliberate violation of each rule.

## Acceptance

- [ ] Nx targets for .NET (build, test, format check) and TypeSpec (compile, generate client)
- [ ] Biome v2 config (size, complexity, boundaries) + minimal ESLint (Nx boundaries) + knip
- [ ] CI: lint, typecheck, test across all toolchains, drift check, error catalogue test, mirrored-test check, size report
- [ ] Each rule fails CI on a deliberate violation
- [ ] Spike answers recorded: does `nx affected` span TypeSpec → client → webapp; which Biome rules are stable enough

## Context

- [Spike plan, Phase 5](../../../architecture/spike-walking-skeleton.md)
- `.github/workflows/ci.yml` (TypeScript only today)

## Tasks

Not planned yet: run `backlog-plan` with source "Phase 5" once S03 is done (S03.T04 adds the first .NET step to CI).

## Summary
