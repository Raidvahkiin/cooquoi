# E01 — Spike walking skeleton

## Goal

Build the thinnest end-to-end slices (authentication, Foodpedia) through every layer and confirm or correct every ADR item marked *(spike)*. One story per spike phase.

## Source

[Spike plan](../../architecture/spike-walking-skeleton.md), sections "Phase 1" to "Phase 5". Phase 4 is split by app: user app (S04) and curator app (S06). Phases 1 and 2 were built before this backlog existed; their findings stay in the spike plan.

## Later (after E01)

- Playwright end-to-end tests for the user and curator apps (decided 2026-11-22 while planning S04)

## Stories

| Id | Title | Status | Depends on |
|---|---|---|---|
| [E01.S01](S01-authentication/story.md) | Authentication end to end (phase 1) | done | — |
| [E01.S02](S02-foodpedia-backend/story.md) | Foodpedia backend (phase 2) | done | E01.S01 |
| [E01.S03](S03-foodpedia-contract/story.md) | Contract for Foodpedia (phase 3) | done | E01.S02 |
| [E01.S04](S04-foodpedia-webapp/story.md) | Foodpedia webapp (phase 4) | done | E01.S03 |
| [E01.S05](S05-tooling-ci/story.md) | Tooling and CI (phase 5) and dev environment | todo | E01.S03 |
| [E01.S07](S07-redis-session-store/story.md) | Redis session store for the webapp BFF | todo | E01.S05 |
| [E01.S06](S06-curator-app/story.md) | Curator app (phase 4, curator side) | todo (T01 planned) | E01.S07 (T01: —) |

Run order: S05 → S07 → S06 (decided 2026-11-22, [S04 findings](S04-foodpedia-webapp/findings.md) "Decisions" 5). S07 comes from [ADR 0019](../../architecture/adr/0019-webapp-sessions-in-redis.md), not from the spike plan; the S05 dev-environment work from [ADR 0018](../../architecture/adr/0018-https-everywhere-in-dev.md).
