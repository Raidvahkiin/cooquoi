# E01.S05.T03 — Tools stop rewriting committed files

## Goal

`next build`, `pnpm run nx:format` and `pnpm biome check .` leave the working tree clean. The webapp has no stale local env file and has a committed `.env.example`.

## Read only

- `biome.json` (`files.includes` :9-32, linter :47-55, assist :56-62)
- `.gitignore` (lines 30-60)
- `package.json:9` (`"nx:format": "biome format . --write"`)
- `ts/apps/cooquoi-webapp/src/shared/config/env.ts:4-13` (env schema, for `.env.example`)
- `ts/apps/cooquoi-webapp/AGENTS.md` "## Commands"

## Reference

Facts found at planning:

- **`next-env.d.ts`**: `ts/apps/cooquoi-webapp/next-env.d.ts` is tracked. `next dev` writes `import "./.next/dev/types/routes.d.ts"` and `next build` writes the non-dev path, so the file flips. Fix: add it to `.gitignore` and `git rm --cached` it; Next regenerates it. Then check that `typecheck` still passes on a fresh clone with no `.next` folder. If it does not, make `typecheck` depend on a step that generates it, and say which in the report.
- **`dotnet/generated/openapi.json`** is written by the .NET build (`dotnet/src/Cooquoi.Api/Cooquoi.Api.csproj:4-6`) in the ASP.NET serializer's format. It is not ignored by Biome (only `contracts/generated` is, `biome.json:20`), so `nx:format` reformats it. Fix: add `"!dotnet/generated"` to `files.includes`.
- **Biome `assist/source/organizeImports`** reports more than the 3 files noted in S04. All paths are under `ts/apps/cooquoi-webapp/`:
  - test files: `src/features/foodpedia/queries/search-ingredients.query.test.ts`, `src/features/household/queries/get-me.query.test.ts`, `src/shared/api/to-api-error.test.ts`, `src/shared/auth/require-session.test.ts`, `src/shared/ui/error-message/error-message.test.tsx`;
  - other files: `src/app/layout.tsx`, `src/shared/i18n/resolve-locale.ts`, `test/msw/handlers/foodpedia.ts`, `test/setup.ts`.

  Fix them with `pnpm biome check --write ts/apps/cooquoi-webapp`, which only reorders imports. Two lint errors are also reported:
  - `src/features/foodpedia/components/ingredient-search/ingredient-search-form.tsx:23:31`, `lint/a11y/useSemanticElements`;
  - `src/shared/api/to-api-error.ts:81:6`, `lint/suspicious/noAssignInExpressions`.

  Fix both by hand, keeping the behaviour.
- **`.env.development.local`** is gitignored (`.gitignore:34`) and untracked. It still holds the old API and issuer ports. Aspire sets every variable, so delete the local file. Add a tracked `ts/apps/cooquoi-webapp/.env.example` that lists the variables from `env.ts` with placeholder values and https URLs ([ADR 0018](../../../architecture/adr/0018-https-everywhere-in-dev.md)), plus a comment that Aspire sets them.

## Skills

`refactor` (only for the two lint fixes)

## Model

haiku (fully specified above)

## Out of scope

- New Biome rules and knip (T05); https wiring (T02); CI (T06)

## Subtasks

- [ ] T03.1 `.gitignore` + untrack `next-env.d.ts`; verify `typecheck` without `.next`
- [ ] T03.2 `biome.json`: ignore `dotnet/generated`
- [ ] T03.3 `biome check --write` for import order, then the two lint errors by hand
- [ ] T03.4 Delete the stale `.env.development.local`, add `.env.example`

## Done when

- `pnpm exec biome check . 2>&1 | tail -3` reports no errors.
- After `pnpm run nx:format && pnpm nx run @cooquoi/webapp:build 2>&1 | tail -3`, `git status --porcelain` lists only this task's intended changes. Rerunning both leaves it unchanged.
- `pnpm nx run-many -t typecheck test -p @cooquoi/webapp 2>&1 | tail -10` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`chore(repo): stop tools rewriting committed files (E01.S05.T03)`), stop.

Status: todo

Report:
