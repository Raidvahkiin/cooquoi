# E01.S03.T01 — Common operation templates, paging, headers, problems

## Goal

`contracts/common/` declares, once, the operation kinds every context reuses: list, read, create, action and replace-action, with paging, `ETag` / `If-Match` / `Idempotency-Key` headers and status-wrapped problems, and the common error codes the backend already returns.

## Read only

- `contracts/common/errors.tsp` (whole file, ~60 lines), `contracts/common/types.tsp`, `contracts/common/main.tsp`
- `contracts/contexts/household/routes.tsp:1-20` (how an operation and its problems are declared today)
- [ADR 0009](../../../architecture/adr/0009-api-contract-typespec.md) Decision §2 to §4
- `dotnet/AGENTS.md` section "Responses and HTTP semantics" (status codes, `ETag`, `If-Match`, idempotency)

## Reference

- Problem pattern: `contracts/common/errors.tsp:34-61` (`ProblemDetails`, `UnauthorizedProblem` with `@statusCode 401` and `application/problem+json`). Copy it for 400, 403, 404, 409, 412, 422, 428.
- Paging shape (matches the backend): `dotnet/src/BuildingBlocks/Raidvahkiin.Ddd.EntityFrameworkCore/Paging/Paged.cs:4` → `Page<T> { items; page: int32; pageSize: int32; totalCount: int64 }`; defaults `page = 1`, `pageSize = 20`, max 100 (ADR 0009 §3).
- Headers: `ETag` is a quoted version `"<n>"` (`dotnet/src/Cooquoi.Platform/Concurrency/IfMatch.cs:67`); missing `If-Match` → 428 `common.if-match-required`, stale → 412 `common.version-mismatch` (`IfMatch.cs:17-27`). `Idempotency-Key` is a UUID, required on every authenticated POST (`dotnet/src/Cooquoi.Platform/Idempotency/IdempotencyMiddleware.cs:17-27`); its errors: `IdempotencyError.cs:8-18` (400 key-required, 422 key-reused, 409 request-in-progress, 409 request-already-processed).
- Field-level codes (`errors[].code`): the shared ones are the values of `dotnet/src/Cooquoi.Platform/Validation/ValidationCodes.cs:8-20` (`validation.required`, `max-length`, `min-length`, `min`, `max`, `range`, `pattern`, `invalid`) plus `validation.one-of`, written as a literal in validators (e.g. `dotnet/src/Cooquoi.Context.Foodpedia/Features/CreateIngredient/CreateIngredient.cs:46-54`) → `union ValidationCode` (9 codes); `FieldError.code` (`errors.tsp:22`) typed with it.
- Codes to add to `CommonErrorCode` (`errors.tsp:8-19`, already drifted): `common.invalid-sort`, `common.idempotency-key-required`, `common.request-already-processed`.

## Skills

none (no `contract` skill yet: T06 writes it from this task's result)

## Lookups

- "In TypeSpec 1.x, how does a templated operation (`op Read<T> is ...` or an `interface` template) add response headers (`@header etag`) and a union of error models, and how does `@typespec/openapi3` emit them?" → expected: a short snippet from the TypeSpec docs (use the context7 MCP, `@typespec/http` templates), ≤10 lines.

## Out of scope

- Foodpedia models and routes (T02); OpenAPI emission in the API (T03); `@typespec/versioning` (keep the `/v1` prefix as today)
- Moving `getMe` onto a template: only if it is a one-line change with an unchanged `openapi.yaml` diff for `/v1/me`

## Subtasks

- [x] T01.1 `common/paging.tsp`: `ListQuery<TFilter>` (`q`, `sort[]`, `page`, `pageSize` with `@maxValue(100)`) and `Page<T>`
- [x] T01.2 `common/headers.tsp`: `ETagHeader`, `IfMatchHeader`, `IdempotencyKeyHeader` (uuid)
- [x] T01.3 `common/errors.tsp`: problem wrappers per status (400, 403, 404, 409, 412, 422, 428) reusing `ProblemDetails`; add the 3 missing common codes; `union ValidationCode` and `FieldError.code: ValidationCode`
- [x] T01.4 `common/operations.tsp`: templates `List<TFilter, TItem>` (200 `Page`, 400, 401), `Read<T>` (200 + `ETag`, 401, 404), `Create<TBody, TCreated>` (`Idempotency-Key`; 201 + `Location` + body; 400, 401, 403, 409, 422), `Action<TBody>` (`POST …:verb`, `Idempotency-Key`; 204 + `ETag`; 401, 403, 404, 409, 422), `ReplaceAction<TBody>` (= `Action` + required `If-Match`; adds 412, 428)
- [x] T01.5 Import the new files from `common/main.tsp`; compile; `openapi.yaml` diff limited to new components and the new codes

## Done when

`pnpm nx run @cooquoi/contracts:compile` passes with no warnings, `git diff contracts/generated/openapi.yaml` shows only additions (no change to `/v1/me`), and `pnpm nx run @cooquoi/webapp:typecheck` is green.

Then: tick the subtasks, write the report (≤5 lines: what changed, verified, left or surprising; note anything the templates could not express cleanly), set the status, commit (`feat(contracts): common operation templates`), stop.

Status: done

Report:
- Added `common/paging.tsp` (`ListQuery`, `Page`), `headers.tsp` (aliases, so no empty schemas), `operations.tsp`; `errors.tsp` gets a `Problem<TStatus>` template + 8 wrappers (401 now `is Problem<401>`, `/v1/me` unchanged), 3 codes, `union ValidationCode` on `FieldError.code`.
- Templates are named `ListOperation`, `ReadOperation`, `CreateOperation`, `ActionOperation`, `ReplaceActionOperation`: `Read`/`Create` clash with TypeSpec visibility templates under `using Cooquoi.Common` (ambiguous-symbol).
- Verified: compile clean (no warnings), `tsp format` clean, openapi diff = 3 codes + `ValidationCode` + `FieldError.code` $ref; webapp typecheck green; a throwaway probe instantiating all 5 templates emitted the expected statuses/headers (`ETag`, `Location`, `Idempotency-Key` uuid, `If-Match`).
- Deviations: `ActionOperation` also returns 400 (validation, missing key); `sort` uses `explode: true` to match ASP.NET `string[]` binding; `page`/`pageSize` have `@minValue(1)`.
- For T02: `Page<T>` instances emit inline (add `@friendlyName` if a named schema is wanted); a filter model of `@query` props emits as `components/parameters/<Ns>.<Filter>`.
