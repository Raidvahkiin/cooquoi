# Backlog

Work split into **epic → story → task → subtask**, written by the `backlog-plan` skill and run by the `backlog-run` skill. Files are small on purpose: a session reads one story file and one task file, never the whole backlog.

## Levels

| Level | Is | Size | Lives in |
|---|---|---|---|
| Epic | An outcome worth several stories (a spike phase, a bounded context's first release) | 2–8 stories | `E0n-<slug>/epic.md` |
| Story | An increment someone can verify on its own (an endpoint set, a screen, a CI rule) | 1–6 tasks | `E0n-<slug>/S0n-<slug>/story.md` |
| Task | One session: one area or toolchain, one commit | ~20–60 tool calls | `…/S0n-<slug>/T0n-<slug>.md` |
| Subtask | One step inside a task (one file, one test group); the unit a subagent may take | a few calls | checklist in the task file |

Ids: `E01`, `E01.S02`, `E01.S02.T03`, subtask `T03.2`. Never renumber; drop with status `dropped`.

Status: `todo` · `doing` · `done` · `blocked (<why>)` · `dropped`.

## Files

**`epic.md`**: `# E0n — <title>`, Goal (2–3 lines), Source (doc and section), Stories table (`id | title | status | depends on`).

**`story.md`**: `# E0n.S0n — <title>`, Goal, Acceptance (checklist; rule ids and use-case ids from the context docs), Context (paths with headings or line ranges, max ~6), Tasks table (`id | title | status | depends on | parallel-safe | session`), Summary (≤5 lines, written when done).

**`T0n-<slug>.md`**: the [task brief](../../.agents/prompts/task-brief.md) sections (Goal, Read only, Reference, Skills, Model, Lookups, Out of scope, Subtasks, Done when), then `Status:` and `Report:` (≤5 lines, written by the worker).

## Epics

| Id | Title | Status |
|---|---|---|
| [E01](E01-spike-walking-skeleton/epic.md) | Spike walking skeleton | done (S06 deferred) |
| [E02](E02-foodventory-api/epic.md) | Foodventory API: manual stock (roadmap step 1) | todo |
| [E03](E03-portable-domain/epic.md) | Portable domain and offline-ready API (before E02.S06) | todo |

## Roadmap after E01

Decided 2026-11-22: **API first, suggestions before UI.** The goal is the core domain (Planning & Suggestion) answering "what can I cook?" through the API, fed by real stock, before any new screen is built.

1. **Foodventory**: model ([docs/contexts/foodventory](../contexts/foodventory/)), then the API with manual stock entry
2. **Product scan / receipt input** into Foodventory (at least the API), or manual add only if scan waits
3. **Cookbook and Market**: the minimum suggestions need (recipes with quantities; prices at preferred vendors)
4. **Planning & Suggestion API**: ranked suggestions ([ADR 0005](../architecture/adr/0005-ai-suggestions-token-strategy.md))
5. **UI** for the above, once the API is ready

Deferred: curator app (E01.S06); curation goes through the API.
