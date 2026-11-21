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

**`T0n-<slug>.md`**: the [task brief](../../.agents/prompts/task-brief.md) sections (Goal, Read only, Reference, Skills, Lookups, Out of scope, Subtasks, Done when), then `Status:` and `Report:` (≤5 lines, written by the worker).

## Epics

| Id | Title | Status |
|---|---|---|
