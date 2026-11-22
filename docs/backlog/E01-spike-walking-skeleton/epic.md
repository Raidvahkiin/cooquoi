# E01 — Spike walking skeleton

## Goal

Build the thinnest end-to-end slices (authentication, Foodpedia) through every layer and confirm or correct every ADR item marked *(spike)*. One story per spike phase.

## Source

[Spike plan](../../architecture/spike-walking-skeleton.md), sections "Phase 1" to "Phase 5". Phases 1 and 2 were built before this backlog existed; their findings stay in the spike plan.

## Stories

| Id | Title | Status | Depends on |
|---|---|---|---|
| [E01.S01](S01-authentication/story.md) | Authentication end to end (phase 1) | done | — |
| [E01.S02](S02-foodpedia-backend/story.md) | Foodpedia backend (phase 2) | done | E01.S01 |
| [E01.S03](S03-foodpedia-contract/story.md) | Contract for Foodpedia (phase 3) | done | E01.S02 |
| [E01.S04](S04-foodpedia-webapp/story.md) | Foodpedia webapp (phase 4) | todo (tasks not planned) | E01.S03 |
| [E01.S05](S05-tooling-ci/story.md) | Tooling and CI (phase 5) | todo (tasks not planned) | E01.S03 |
