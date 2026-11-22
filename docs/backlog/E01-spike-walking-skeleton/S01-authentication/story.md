# E01.S01 — Authentication end to end (phase 1)

## Goal

Sign in through Keycloak, provision the user profile and personal household on first sign-in, show the signed-in user in the webapp.

## Acceptance

- [x] `aspire run` starts Postgres, Keycloak, the API and the webapp
- [x] Signing in creates a profile and a household (ADR 0015); repeat sign-in keeps the same ids
- [x] Concurrent first sign-ins → one profile, one household
- [x] Webapp login, callback, logout and token refresh tested

## Context

- [Spike plan, Phase 1](../../../architecture/spike-walking-skeleton.md) ("Status" and "Answers so far")

## Tasks

Built before the backlog existed; no task files.

## Summary

Done 2026-11-21. Findings recorded in the spike plan. Left for later stories: Astryx `AppShell` (S04), Google brokering (needs a Google OAuth client), webapp browser-mode project (S04).
