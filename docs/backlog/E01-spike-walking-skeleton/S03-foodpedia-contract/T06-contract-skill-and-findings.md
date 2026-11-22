# E01.S03.T06 — `contract` skill and phase 3 findings

## Goal

A `contract` project skill tells a worker how to add an operation to the TypeSpec contract with the real templates, and the spike plan records phase 3's status and answers.

## Read only

- The reports of T01–T05 in this folder (their `Report:` lines)
- `contracts/common/operations.tsp` and `contracts/contexts/foodpedia/routes.tsp` (the reference to quote)
- `.agents/skills/dotnet-feature/SKILL.md:1-30` (skill format: frontmatter, steps, reference implementation)
- [Spike plan](../../../architecture/spike-walking-skeleton.md) "Phase 2" status block (format to copy) and "Phase 3"
- `AGENTS.md` (root) "Project skills" table

## Reference

- Skill format: `.agents/skills/dotnet-feature/SKILL.md:1-12` (frontmatter `name`, `description` with "Use when…", then Steps)
- Status/answers format: Phase 2's "Status (2026-11-21)" and "Answers" in the spike plan

## Skills

none (follow the format of the reference skill)

## Out of scope

- ADR updates (spike plan "After the spike" item 1, done once for all phases); other skills; any code change

## Subtasks

- [x] T06.1 `.agents/skills/contract/SKILL.md`: when to use, layout (`common/`, `contexts/<name>/{main,models,routes,errors}.tsp`), which template per operation kind, error codes per context, regenerate (`api-client:generate`), drift (`contracts:drift`) and catalogue test, then `api-client-sync`; reference = Foodpedia routes
- [x] T06.2 Link it: `.claude/skills/contract` link like the others; row in the root `AGENTS.md` "Project skills" table; `dotnet-feature` step 1 points to it
- [x] T06.3 Spike plan Phase 3: "Status (date)" block and "Answers" (templates, drift noise) from the task reports; mark item 3 of "After the spike" for `contract` done

## Done when

`ls -l .claude/skills/contract` resolves to `.agents/skills/contract`, the root `AGENTS.md` skills table lists `contract`, and the spike plan's Phase 3 has a Status block and both answers.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`docs(repo): contract skill and phase 3 findings`), stop.

Status: done

Report:
- Added `.agents/skills/contract/SKILL.md` (layout, template per kind with answers, error codes, generate → `dotnet build` → `contracts:drift` → catalogue test → `api-client-sync`, `[ContractCodes]`/`[ContractEnum<T>]` pitfall; reference = Foodpedia routes), linked from `.claude/skills/contract`, root `AGENTS.md` table and `dotnet-feature` step 1.
- Spike plan Phase 3: Status (2026-11-22), both answers and a Decision line (ADR 0009 holds, update after the spike); "After the spike" item 3 marks `contract` done.
- Verified: symlink resolves, skill is picked up by the harness, Done-when greps match, ADR link target exists. Docs only, no code touched.
