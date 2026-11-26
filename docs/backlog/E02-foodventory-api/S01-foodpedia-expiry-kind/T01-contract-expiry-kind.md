# E02.S01.T01 — Contract: expiry kind and `:set-expiry-kind`

## Goal

The TypeSpec contract has an `ExpiryKind` union. `expiryKind` is required in the create body and present in `IngredientDetail`, and `setExpiryKind` is a replace action. The client is regenerated and the webapp still typechecks and tests green.

## Read only

- `.claude/skills/contract/SKILL.md` (steps 1–4)
- [Foodpedia use cases](../../../contexts/foodpedia/use-cases.md) C1, C11
- `contracts/contexts/foodpedia/models.tsp:16` (`union Storage`), `:53-67` (ShelfLife, IngredientDetail), `:94` (create body)
- `contracts/contexts/foodpedia/routes.tsp:35-38`
- `contracts/common/operations.tsp:70-90` (ReplaceActionOperation)
- `ts/apps/cooquoi-webapp/AGENTS.md` "Tests" (MSW fixtures)

## Reference

- Union: copy `union Storage` (`models.tsp:16`) as `union ExpiryKind { printed: "printed", estimated: "estimated", longLife: "long-life" }`.
- Route: copy `defineConversion` (`routes.tsp:35-38`): `@route("/v1/ingredients/{ingredientId}:set-expiry-kind") @operationId("setExpiryKind") setExpiryKind is ReplaceActionOperation<SetExpiryKind, IngredientPath>;` with body `{ expiryKind: ExpiryKind }`. Doc comment: "C11 … Requires the curator role. Errors: `foodpedia.ingredientRetired` (422)."
- Add `expiryKind` to `IngredientSummary` (`models.tsp:37`) only if the webapp list needs it. It does not in E02, so leave it out.
- No new error code: F12 is a required enum (`validation.*`), and retired is the existing `foodpedia.ingredientRetired`.

## Skills

`contract`, `api-client-sync` (fixtures only)

## Model

haiku (fully specified by Reference)

## Out of scope

C# code (T02, T03), PublicApi (T04), webapp screens

## Subtasks

- [x] T01.1 `ExpiryKind` union; `expiryKind` in `IngredientDetail` and the create body (required)
- [x] T01.2 `setExpiryKind` route and `SetExpiryKind` model
- [x] T01.3 `pnpm nx run @cooquoi/api-client:generate`; commit `contracts/generated/openapi.yaml` and the client
- [x] T01.4 Webapp MSW fixtures / test data that build `IngredientDetail` get `expiryKind` (no screen change)

## Done when

`pnpm nx run-many -t typecheck test lint -p @cooquoi/webapp @cooquoi/api-client @cooquoi/contracts 2>&1 | tail -5` is green. The drift check is expected red until T03.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(contracts): Foodpedia expiry kind (E02.S01.T01)`), stop.

Status: done

Report:
- Changed: `ExpiryKind` union, `expiryKind` on `IngredientDetail` and required on `CreateIngredient`, `SetExpiryKind` model, `setExpiryKind` route in `contracts/contexts/foodpedia/`; regenerated `contracts/generated/openapi.yaml` and `ts/libs/api-client/src/generated/`.
- Verified: Done-when command exit 0; knip on the three projects clean. Drift check not run (expected red until T03).
- T01.4: no fixture builds `IngredientDetail` (only `IngredientSummary`), so no fixture change.
- Surprising: `expiryKind` is now required on the create body, so the C# create endpoint rejects a body without it until T02 lands.
