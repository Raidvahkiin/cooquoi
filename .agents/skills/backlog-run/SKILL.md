---
name: backlog-run
description: Run a story or task from docs/backlog/ token-efficiently — the parent session keeps a small context and hands each task to a fresh session or subagent with a self-contained brief, then verifies it from its report, diff stat and done-when command, and closes the story with a review. Also holds the worker rules for a session running one task. Use when asked to run, do, execute or continue a story, task or backlog item.
---

# Backlog run

Format: `docs/backlog/README.md`. Brief shape: `.agents/prompts/task-brief.md`.

Why: every tool call re-reads the whole context. A parent that only reads plans and reports stays near its starting size; each worker starts fresh and grows only for its own task.

## Parent: running a story

### 1. Load

Read `story.md` and the task files of the open tasks. Nothing else: no code, no ADRs, no other stories. If a task file lacks Reference or Done when, fix it first (`backlog-plan` §4); a vague brief makes the worker explore.

### 2. Choose how each task runs

| Run it | When |
|---|---|
| **Here** | Under ~10 tool calls, and this session is still small |
| **Explore** subagent | A question, not a change: where or how something is done across files, what exists, which command verifies an area. Ask for `file:line` plus ≤10 lines, no file contents; `medium` breadth unless the question spans several naming conventions |
| **Subagent** (`Agent`, general-purpose) | Self-contained work whose result is needed now, no steering expected: a subtask, a test group, the review |
| **Session** (fresh) | A normal task (~20–60 calls) or anything the user may want to watch or steer |

- Session tool available (desktop app): start one per task, `context: fresh`, brief = the task brief pointing at the task file. Otherwise print the filled brief for the user to paste into a new session, and stop.
- Parallel only for `parallel-safe` tasks with no file overlap; parallel writers get their own worktree. Default: sequential in dependency order.
- Set the task `Status: doing` and its session name in the story table before starting it.

### 3. Verify each task (cheaply)

1. Read the task's **Report** and status.
2. `git diff --stat` (or the worker's branch) and run the **Done when** command in its quiet form.
3. Open a file only when the stat or the command looks wrong. Never re-read the whole diff in the parent.
4. Red or incomplete → send the same session a short follow-up (what failed, the command output tail), not a new session.

### 4. Close the story

1. Review in a subagent with the matching skill (`dotnet-review`, `webapp-review`) on the story's diff; ask for findings only (≤10 lines). Fix findings through the owning task's session or a subagent.
2. Tick the acceptance criteria you verified, write the story **Summary** (≤5 lines), set statuses in `story.md` and `epic.md`.
3. Report to the user in a few lines; suggest the next story. Stop.

### Budget rule

If this parent session has grown long (many tasks verified, large outputs), write the state into `story.md` (statuses, what is next) and stop: a new parent session continues from the file.

## Worker: running one task

1. Read only the task file and its **Read only** list. Invoke the skills it names.
2. Run the task's **Lookups** first, each as an `Explore` subagent (`file:line` plus ≤10 lines, no file contents). Then do the subtasks in order; tick each `- [x]` as it passes. A new question that spans more than ~3 files or you cannot place with one `grep` → `Explore` the same way. A file you are about to edit, or one named in Reference, you read yourself.
3. Stay out of scope. Something needed outside it → stop and record it in the report as `blocked (<why>)`, do not widen the task.
4. Run **Done when** (quiet form), then the `refactor` skill's verification on what changed.
5. Write **Report** (≤5 lines: changed, verified, left or surprising) and **Status**. Commit only if the brief or the parent says so: one commit per task, conventional message with the task id.
6. Stop. Do not start the next task.
