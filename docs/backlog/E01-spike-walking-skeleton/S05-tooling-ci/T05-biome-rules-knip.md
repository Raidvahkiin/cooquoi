# E01.S05.T05 — Biome rules (size, complexity, boundaries) and knip

## Goal

Biome enforces the webapp's clean-code limits (file and function size, complexity, import cycles, feature boundaries), knip reports unused files, exports and dependencies, and both pass on the current code. Each is reachable as an Nx target so CI (T06) can run it.

## Read only

- `ts/apps/cooquoi-webapp/AGENTS.md` "## Clean code" (lines 109-123), "### Dependency rules (enforced by Biome `noRestrictedImports`)" (line 51), "## Tooling" and "## Commands" (lines 139-155)
- `biome.json` (as left by T03)
- `eslint.config.mts` (Nx boundaries only, keep it so)
- `pnpm-workspace.yaml` catalog (lines 8-28)

## Reference

- Limits to encode:
  - files ≤ ~150 lines, functions ≤ ~30 lines;
  - cognitive complexity (`complexity/noExcessiveCognitiveComplexity`);
  - import cycles (`nursery/noImportCycles` or its stable name);
  - feature boundaries per `AGENTS.md` "Dependency rules" (`noRestrictedImports` overrides per folder).
- Biome has no "max lines" rule: check the current Biome 2 docs for `noExcessiveLinesPerFunction` and file-length options. A limit Biome cannot express goes into the size report (T06), not a custom script here.
- knip: add `knip` to the root devDependencies via the catalog, and a `knip.json` with workspaces for `ts/apps/cooquoi-webapp`, `ts/libs/api-client` and `contracts`. The Next.js and Vitest plugins are auto-detected. Ignore generated code (`ts/libs/api-client/src/generated`).
- Nx: `lint` stays ESLint. Add root-level targets (or `package.json` scripts wrapped as Nx targets) named `biome-check` and `knip`, using the inline-target pattern of `contracts/package.json:22-31`.

## Skills

`refactor` (fixing what the new rules flag)

## Model

sonnet

## Out of scope

- CI wiring and violation checks (T06); .NET targets (T04)

## Subtasks

- [x] T05.1 `biome.json`: size, complexity, cycles and boundary rules. Fix or justify (`biome-ignore` with a reason) every hit in the current code.
- [x] T05.2 knip config and dependency. Remove what it finds unused, or list a justified ignore.
- [x] T05.3 Nx targets `biome-check` and `knip`. Update the `AGENTS.md` "Commands" if the command changes.
- [x] T05.4 Note in the report which Biome rules are stable and which are nursery (spike answer).

## Done when

`pnpm nx run-many -t biome-check knip lint typecheck test 2>&1 | tail -10` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`build(repo): Biome size and boundary rules, knip (E01.S05.T05)`), stop.

Status: done

Report:
Report:
Added to `biome.json`: file <=150, function <=30, cognitive complexity <=15, max 2 params, `noImportCycles`, and `noRestrictedImports` per folder (app, features, shared); tests and `scripts/` have lighter overrides. No existing code was flagged.
Added `knip` (catalog, root devDependency) and `knip.json`. Removed the dead i18n barrel, unused exports, `@nx/vite`, `@types/react-dom`; ignores are the legacy folder, generated client, TypeSpec peer deps, vite test aliases, the theme entry and session.ts types.
Nx targets `@cooquoi/monorepo:biome-check` and `:knip` live in the root `package.json`; webapp AGENTS.md Commands updated.
Spike answer: every rule used is stable in Biome 2.6.8 (none nursery): `noExcessiveLinesPerFile`, `noExcessiveLinesPerFunction`, `noExcessiveCognitiveComplexity`, `useMaxParams`, `noImportCycles`, `noRestrictedImports`.
Verified: `nx run-many -t biome-check knip lint typecheck test` green.
