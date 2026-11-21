Run task <E0n.S0n.T0n> — <title>. Follow the worker rules of the backlog-run skill.

Task file: docs/backlog/<E0n-epic>/<S0n-story>/<T0n-task>.md
(Without a task file, the brief itself carries these sections:)

Goal: <one sentence, the outcome, in glossary terms>
Read only: <paths with line ranges or headings; the AGENTS.md section that applies>
Reference: <existing code to copy the pattern from, file:line>
Skills: <dotnet-feature, dotnet-test, webapp-feature…>
Lookups (optional, each run as an Explore agent, answer = file:line + ≤10 lines, no file contents): <question>

Out of scope: <neighbouring work this task must not touch>
Subtasks:
- [ ] <step 1: one file or one test group>
- [ ] <step 2>
Done when: <command that proves it, in its quiet form> and <observable result>
Then: tick the subtasks, write the report (≤5 lines: what changed, what was verified, what is left or surprising), set the status, <commit | do not commit>, stop.
