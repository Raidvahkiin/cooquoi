# S05 findings: tooling and CI

## Violation checks

Each violation was introduced locally, run through the Nx target CI uses (`--skip-nx-cache`), and reverted.

| Rule | Violation | Command | Result |
|---|---|---|---|
| ESLint boundary | `ts/libs/api-client` imports `ts/apps/cooquoi-webapp` by relative path | `pnpm nx run @cooquoi/api-client:lint` | fails, `@nx/enforce-module-boundaries` |
| Biome size | 160-line file | `pnpm nx run @cooquoi/monorepo:biome-check` | fails, `noExcessiveLinesPerFile` (160 > 150) |
| knip unused export | extra `export const` in `search-ingredients.query.ts` | `pnpm nx run @cooquoi/monorepo:knip` | fails, "Unused exports (1)" |
| TypeScript type error | `const bad: number = 'text'` | `pnpm nx run @cooquoi/webapp:typecheck` | fails, TS2322 |
| Failing Vitest test | `expect(1).toBe(2)` | `pnpm nx run @cooquoi/webapp:test` | fails, 1 of 65 |
| Failing .NET test | `1.ShouldBe(2)` in Architecture.Tests | `pnpm nx run @cooquoi/dotnet:test` | fails |
| `dotnet format` | `namespace   X;` extra spaces | `pnpm nx run @cooquoi/dotnet:format-check` | fails, `WHITESPACE` |
| Contract drift | foodpedia route renamed in `routes.tsp` | `pnpm nx run @cooquoi/contracts:drift` | fails (oasdiff + drift gate) |
| Error catalogue | `code-taken` removed from `messages/fr.json` | `pnpm nx run @cooquoi/dotnet:test` | fails, `Contract_codes_are_translated_in_every_locale(fr)` |
| Mirrored test | `Features/Violation/` use case without tests | `pnpm nx run @cooquoi/dotnet:test` | fails, `Use_cases_have_a_mirrored_test_file` |

## Spike answers

- `nx affected` spans toolchains (T04): a `contracts/contexts/foodpedia/routes.tsp` change affects contracts, api-client and webapp; a C# change affects only `@cooquoi/dotnet`. There is no graph edge contracts to dotnet on purpose, so the `contract-drift` job runs `@cooquoi/contracts:drift` on every change.
- Biome (T05): size, complexity, `useMaxParams`, `noImportCycles` and restricted-import rules are all stable (no nursery); size limits are errors.
- One `verify` job: `nx affected` on pull requests, `nx run-many` on main and tags, for `lint typecheck test biome-check knip build format-check`. The .NET tests (architecture, error catalogue, mirrored tests) run inside `@cooquoi/dotnet:test`.
- Mirrored-test check is an architecture test (`MirroredTestRulesTests`): aggregates and value objects (domain types declaring their own behaviour; ids, events, enums, plain data carriers exempt) need `<Type>Tests.cs`; each `Features/<UseCase>/` needs a `*Tests.cs` in the mirrored folder.
- The size report (step summary) only reports: Biome size rules are already errors.

## What is left

- `ci.yml` is linted with actionlint and parses, but has not run on GitHub (`nrwl/nx-set-shas@v5` and the Docker-dependent drift and Testcontainers steps are untested there).
- Biome's size and complexity rules are errors and `biome check .` reports no diagnostics; test files only drop the function-size and max-params rules, and keep a 300-line file cap.
- `search-ingredients.query.test.ts` is 170 lines (test files have a 300-line file cap instead of 150).
- Playwright e2e is out of scope (epic "Later").
