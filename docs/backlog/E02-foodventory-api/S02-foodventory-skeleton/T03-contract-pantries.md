# E02.S02.T03 — Contract: Foodventory namespace, Pantry aggregate operations and errors

## Goal

`contracts/contexts/foodventory/` declares the pantry operations C1–C4 and Q1, the models, and the codes `foodventory.name-taken`, `foodventory.invalid-name` and `foodventory.not-empty`, with messages in the webapp catalogues. It adds two common templates: a non-paged read of a collection and a delete with `If-Match`. The client is regenerated.

## Read only

- `.claude/skills/contract/SKILL.md` (steps 1–4)
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) Q1, "Commands — pantries", last paragraph of "Commands — stock" (If-Match, Idempotency-Key)
- `contracts/main.tsp:3-15`, `contracts/contexts/foodpedia/{main,errors,routes}.tsp` (structure), `models.tsp:16` (union)
- `contracts/common/operations.tsp:18-90` (List, Read, Create, Action, ReplaceAction)
- `ts/apps/cooquoi-webapp/messages/en.json` `errors` (shape of an error message entry)
- Open questions Q4, Q5 ([epic](../epic.md#open-questions))

## Reference

- Folder: copy `contracts/contexts/foodpedia/` (main, models, routes, errors); import it in `contracts/main.tsp:3-5`. `@tag("Foodventory")`.
- Routes:
  - `GET /v1/pantries` → `PantryList { pantries: PantrySummary[], toPutAwayCount: int32 }`, where `PantrySummary = { id, name, storage?, itemCount, version }`. New template `CollectionReadOperation<T>` in `operations.tsp`: 200 + body, no paging. Q1 is small, not paged.
  - `POST /v1/pantries` `CreateOperation<CreatePantry, CreatedPantry>` (`{ name, storage? }`).
  - `POST /v1/pantries/{pantryId}:rename` `ReplaceActionOperation<RenamePantry, PantryPath>`.
  - `POST /v1/pantries/{pantryId}:set-storage` `ReplaceActionOperation<SetPantryStorage, PantryPath>` (`storage` nullable).
  - `DELETE /v1/pantries/{pantryId}`: new template `DeleteOperation<TPath>` with `If-Match` and 204, the problems of `ReplaceActionOperation` (`operations.tsp:70-90`).
- `union Storage { fridge, freezer, room }`, the Foodventory context's own type.
- Errors: `union FoodventoryErrorCode { nameTaken: "foodventory.name-taken", invalidName: "foodventory.invalid-name", notEmpty: "foodventory.not-empty" }`. Doc comments on each route list its codes (FV1 → 409 / 400, FV13 → 422).
- Messages: `errors.foodventory.{name-taken,invalid-name,not-empty}` in `en`, `fr`, `vi`, copying the shape of the `foodpedia` entries.

## Skills

`contract`

## Model

sonnet (two new templates)

## Lookups

- Does [ADR 0009](../../../architecture/adr/0009-api-contract-typespec.md) "Decision" allow `DELETE`, or does it require named actions only? Expected: ≤5 lines. If it requires named actions, use `POST /v1/pantries/{pantryId}:delete` as a `ReplaceActionOperation` and say so in the report.

## Out of scope

C# code (T04, E02.S03); stock operations (E02.S04.T01)

## Subtasks

- [ ] T03.1 Folder, `main.tsp` import, `Storage`, models
- [ ] T03.2 `CollectionReadOperation` and `DeleteOperation` templates
- [ ] T03.3 Five routes with doc comments (use-case ids C1–C4, Q1)
- [ ] T03.4 Error union and webapp messages (en, fr, vi)
- [ ] T03.5 `pnpm nx run @cooquoi/api-client:generate`; commit `openapi.yaml` and the client

## Done when

`pnpm nx run-many -t typecheck test lint -p @cooquoi/webapp @cooquoi/api-client @cooquoi/contracts 2>&1 | tail -5` is green. Drift and the error-catalogue test are expected red until E02.S02.T04 / E02.S03.T02.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(contracts): Foodventory pantry operations (E02.S02.T03)`), stop.

Status: todo

Report:
