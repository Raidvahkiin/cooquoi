---
name: refactor
description: Refactor code in the Cooquoi repo (TypeScript or C#) with a 2-level boy-scout scope, fix-or-justify for every smell, and a mandatory verification loop that removes dead code and pass-through layers. Use after implementing any change, when asked to refactor or clean up, and as the last step of feature skills.
---

# Refactor

Refactoring is the **default**, not an option. Leaving a smell in scope requires an allowed reason.

## 0. Preconditions

1. Run `lint`, `typecheck` and `test` through Nx on the affected projects. **If anything fails, stop and report** — never refactor on a red build.
2. Note the files changed by the task: this is **level 0**.

## 1. Scope — boy scout up to 2 levels

| Level | Scope |
|---|---|
| 0 | Files changed by the task |
| 1 | What level-0 files import, and what imports them |
| 2 | Direct imports and importers of level-1 files |

Limits:

- Stay inside the same feature (webapp) or bounded context (backend). Crossing into `shared/` or another context is allowed only to remove dead code or a pass-through layer, and must be flagged in the report.
- Never change a public contract (API contract, a feature barrel export used elsewhere, a context's published queries/events) without explicit approval.

## 2. Find and fix smells

Walk every file in scope against this catalogue. For each smell: **fix it**, or record a **skip with an allowed reason**.

| Smell | Fix |
|---|---|
| Function > ~30 lines, file > ~150 lines | Extract functions / split the file |
| > 2 parameters | Parameter object |
| Duplicate logic (2+ copies) | Extract one function at the lowest common level |
| Logic in a page or route (webapp) / in an endpoint (backend) | Move to query, action, view model / handler or domain |
| Business rule in a handler (backend) | Move into the aggregate or a value object |
| Vague names (`data`, `handle`, `utils`, `temp`, `manager`) | Rename to intent |
| Deep nesting, complex condition | Guard clauses, named predicate (`isExpiringSoon`) |
| Primitive carrying domain meaning | Union type / enum / value object / strongly typed id |
| Feature envy | Move the function to the module whose data it uses |
| Comment explaining *what* | Rename or extract so the code says it; delete the comment |

**Allowed skip reasons (only these):**

1. Outside the 2-level scope — listed as a follow-up.
2. Requires a public contract change — needs approval.
3. No test covers it — add a test first, then refactor; skip only if a test cannot reasonably be written now.
4. Conflicts with an ADR or a convention — name the ADR.

"It works", "minor", "keep the diff small" and "out of time" are **not** valid reasons.

## 3. Verification loop (mandatory)

Refactoring creates new dead code and useless layers. After the fixes, run this pass. **Repeat until a pass finds nothing new, maximum 3 passes.** Report anything left after pass 3.

| Detect | How | Fix |
|---|---|---|
| Dead code: unused files, exports, functions, types, dependencies, i18n keys, CSS classes | TS: `pnpm knip` + Biome unused-variable/import rules. C#: analyzers IDE0051/IDE0052/IDE0060 + warnings as errors; search references for public members | Delete. Never comment out, never keep "just in case" |
| Pass-through function or class: body only forwards its arguments to another call with the same meaning, or a wrapper adding nothing | Would removing it change anything except call sites? If not, it is a pass-through | Inline it, update callers |
| Speculative layer: interface with one implementation and no test/ADR reason, generic helper used once | Count usages | Inline the implementation / make the helper local |
| Barrel exports nobody imports | `knip` | Remove the export |
| Tests of deleted or inlined code | Tests importing removed symbols | Delete or move to the new location; behaviour coverage must not drop |
| Leftovers: empty folders, stale mocks/fixtures, docs or comments with old names | Search for the old symbol names | Clean up |

## 4. Safety

- Behaviour is preserved: the same tests pass with unchanged assertions (only imports and locations may change).
- Never weaken production code for tests (no extra parameters or injected seams; use module or network mocks).
- Final run through Nx on affected projects: `lint`, `typecheck`, `test`, plus `biome check` and `knip` (TS) or `dotnet build` with analyzers (C#).
- Refactoring is committed separately from behaviour changes, with a `refactor:` commit type.

## 5. Report

Fill in [report-template.md](report-template.md) and include it in the final message (and in the PR description when there is one).
