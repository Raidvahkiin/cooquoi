# Prompt templates

Copy one, fill the `<…>` parts, paste it as the first message of a session. Agents use the same templates when they write briefs for other sessions or subagents.

| Template | Use it to |
|---|---|
| [plan.md](plan.md) | Start a planning session: turn a doc or an idea into epics, stories and tasks in `docs/backlog/` (`backlog-plan` skill) |
| [run-story.md](run-story.md) | Start a parent session that runs a story task by task (`backlog-run` skill) |
| [task-brief.md](task-brief.md) | Start a worker session or subagent on one task; also the shape of every task file |
| [quick.md](quick.md) | A small change that needs no backlog entry |

Rules every template follows: name what to read (paths and line ranges or headings), what is out of scope, what "done" is (a command that proves it), and when to stop.
