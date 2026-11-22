# E01.S06 — Curator app (phase 4, curator side)

## Goal

A second Next.js app for curators, separate from the user app `@cooquoi/webapp`, where a curator defines an ingredient conversion (C5) through a Server Action with `If-Match`; a stale version shows a translated error by code. Its shape (name, path, auth, shared code) is first decided in an ADR.

## Acceptance

- [ ] ADR 0017 (curator app) accepted: app name and path, sign-in and role guard (`catalog-curator`), what moves to `ts/libs` to be shared with the user app, Aspire and deployment
- [ ] The curator app runs under Aspire; a user without `catalog-curator` cannot use it
- [ ] Q2 ingredient page and C5 define-conversion through a Server Action with `If-Match` and `Idempotency-Key`; a stale version shows `common.version-mismatch` translated by code
- [ ] The error catalogue test covers the curator app's message catalogues too
- [ ] Tests: Vitest Node (queries, actions, view models + MSW) and browser mode (components)

## Context

- [Spike plan, Phase 4](../../../architecture/spike-walking-skeleton.md) (lines 145-160)
- [S04](../S04-foodpedia-webapp/story.md): the user app's i18n, theme, AppShell, error display and browser-mode setup this app reuses
- [S04.T06](../S04-foodpedia-webapp/T06-define-conversion.md) (dropped): the define-conversion brief to start from once the ADR is decided
- [Foodpedia use cases](../../../contexts/foodpedia/use-cases.md) Q2, C5 (lines 11, 19, 34); actors table (lines 6-12)
- [ADR 0011 Decision](../../../architecture/adr/0011-webapp-api-auth.md), [ADR 0013 Decision](../../../architecture/adr/0013-webapp-structure.md)

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-curator-app-adr.md) | ADR 0017: curator app | todo | — | yes | s06-t01 |

The other tasks (scaffold the app, extract shared code to `ts/libs`, define-conversion from S04.T06, catalogue check for both apps, findings) are not planned yet: run `backlog-plan` with source "ADR 0017" once T01 is accepted and S04 is done. T01 only writes docs, so it can run alongside S04.

## Summary
