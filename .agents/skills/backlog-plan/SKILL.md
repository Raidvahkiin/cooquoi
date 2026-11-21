---
name: backlog-plan
description: Analyse a source (spike plan phase, context use cases, vision journey, or a described feature) and the current code, then write epics, stories, tasks and subtasks into docs/backlog/ as small self-contained files sized for token-efficient sessions. Planning only, no implementation. Use when asked to plan, break down, prepare tasks or a backlog, or before starting a phase or feature.
---

# Backlog plan

Format, sizes, ids and statuses: `docs/backlog/README.md` (read it first, it is short). Task files follow `.agents/prompts/task-brief.md`.

Output is files, not code. The quality bar: a worker that reads **only** one task file can do the task without exploring.

## 1. Scope

1. Read the source narrowly: the named section, not the whole doc (`grep -n '^#'` first).
2. Read `docs/backlog/README.md` Epics table and, if extending an epic, its `epic.md`. Reuse ids; never renumber.
3. Pick only the ADRs whose title in `docs/README.md` matches the work; read their **Decision** section only.

## 2. What exists

Use one `Explore` subagent to map the current state; ask for `file:line` conclusions only:
- what already exists for this scope, and the closest **reference implementation** per kind of work (endpoint, query, screen, contract route, test);
- which projects and Nx / dotnet commands verify each area.

Do not read the code yourself beyond a reference you must quote.

## 3. Decompose

- **Story** = verifiable increment. Acceptance criteria name rule ids (F1…) and use-case ids (Q1, C5…) from the context docs, or an observable check.
- **Task** = one session: one area (contract, backend context, webapp feature, CI), ~20–60 tool calls, one commit. Split a task that crosses toolchains, needs more than ~6 files to read, or has more than ~8 subtasks. Merge tasks under ~10 calls into a neighbour.
- **Subtask** = ordered step inside the task (one file or one test group), checkable.
- Order by dependency. Mark `parallel-safe: yes` only when the task's files cannot overlap another open task's files.
- Put the contract before the code that depends on it, tests in the same task as the code they cover, and the review in the story's last step (run by `backlog-run`, not a task).

## 4. Write each task file

Every section of the brief, concrete:
- **Read only**: paths with headings or line ranges, max ~6 entries, including the `AGENTS.md` section that applies.
- **Reference**: `file:line` of the pattern to copy.
- **Skills**: the project skills the worker invokes (`dotnet-feature`, `webapp-feature`, `api-client-sync`…).
- **Lookups** (optional): questions the worker must answer before coding that you could not settle with the step 2 map, each phrased as an `Explore` query with the expected answer (`file:line`, ≤10 lines). Prefer settling it now and putting the answer in Reference.
- **Out of scope**: the neighbouring tasks, by id.
- **Done when**: the quiet verification command from `AGENTS.md` plus an observable result.

## 5. Finish

1. Update the Epics table (README), `epic.md` and each `story.md` tasks table.
2. Self-check: every task has Reference and Done when; no task depends on a later one; no story without acceptance criteria; open questions listed, not guessed.
3. Report: the story table (id, title, tasks, depends on) and the open questions. Stop; do not start the work.
