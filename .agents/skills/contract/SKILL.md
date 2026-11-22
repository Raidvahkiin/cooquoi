---
name: contract
description: Add or change an operation, model or error code in the Cooquoi TypeSpec API contract (`contracts/`) with the shared operation templates (list, read, create, action, replace-action), declare error codes per context, regenerate the client, and keep the ASP.NET OpenAPI document in step through the drift check and the error-catalogue test. Use when adding or changing an endpoint's route, request, response or error codes, before or with the backend (dotnet-feature) and the webapp (api-client-sync).
---

# Contract

The TypeSpec contract is the source of truth ([ADR 0009](../../../docs/architecture/adr/0009-api-contract-typespec.md)): it compiles to `contracts/generated/openapi.yaml`, which generates the Hey API client (`ts/libs/api-client`). The C# API writes its own document (`dotnet/generated/openapi.json`) at build time; the drift check compares both. Reference implementation: `contracts/contexts/foodpedia/` (`routes.tsp` uses four of the five templates).

## Layout

| File | Content |
|---|---|
| `contracts/main.tsp` | Service, `@info` version, server, auth; imports `common/main.tsp` and each `contexts/<name>/main.tsp` |
| `common/operations.tsp` | The five operation templates (below) |
| `common/errors.tsp` | `ProblemDetails`, `FieldError`, `Problem<TStatus>` and its wrappers (`BadRequestProblem` … `PreconditionRequiredProblem`), `CommonErrorCode`, `ValidationCode` |
| `common/paging.tsp`, `headers.tsp`, `types.tsp` | `ListQuery<TFilter>`, `Page<T>`; header aliases (`ETagHeader`, `IfMatchHeader`, `IdempotencyKeyHeader`); `Id`, `Unit`, `BaseUnit`… |
| `contexts/<name>/main.tsp` | Imports `models.tsp`, `errors.tsp`, `routes.tsp` (nothing else) |
| `contexts/<name>/models.tsp` | Request, response and filter models; `namespace Cooquoi.<Name>` |
| `contexts/<name>/errors.tsp` | `union <Name>ErrorCode { codeTaken: "<name>.code-taken", … }` |
| `contexts/<name>/routes.tsp` | One `@tag`ged `interface <Resource>Operations`; each op = route + `operationId` + template |

A new context: add the folder with these four files and import its `main.tsp` from `contracts/main.tsp`.

## Templates (`common/operations.tsp`)

Named `…Operation` because `Read`/`Create` clash with TypeSpec visibility templates. `TPath` names the path parameter exactly as the backend route does (default `{ @path id: Id }`), e.g. `alias IngredientPath = { @path ingredientId: Id };`.

| Kind | Template | Answers |
|---|---|---|
| List (Q) | `ListOperation<TFilter, TItem>` | `Page<TItem>`; 400, 401. `TFilter` is a model of `@query` props |
| Read one (Q) | `ReadOperation<T, TPath>` | 200 + `ETag`; 401, 404 |
| Create (C) | `CreateOperation<TBody, TCreated>` | `Idempotency-Key`; 201 + `Location` + `{ id }`; 400, 401, 403, 409, 422 |
| Action `POST /…/{id}:verb` | `ActionOperation<TBody, TPath>` | `Idempotency-Key`; 204 + `ETag`; 400, 401, 403, 404, 409, 422 |
| Replace-style action | `ReplaceActionOperation<TBody, TPath>` | as action + `If-Match`; also 412, 428 |

```tsp
@tag("Foodpedia")
interface IngredientOperations {
  /** C5: defines or replaces the conversion of one unit. Requires the curator role. */
  @route("/v1/ingredients/{ingredientId}:define-conversion")
  @operationId("defineConversion")
  defineConversion is ReplaceActionOperation<DefineConversion, IngredientPath>;
}
```

The use-case id (Q1, C5…) from `docs/contexts/<context>/use-cases.md` starts the doc comment; name the error codes it can answer there. An operation that fits no template: write it inline with the same `Problem` wrappers, and say why in its comment.

## Steps

1. **Model.** Add request/response/filter models to `models.tsp`, names from the glossary. Amounts are `decimal`. A string with a closed set of values is a TypeSpec enum or union (it becomes an OpenAPI `enum`). A field the API always sends but may be `null` is **required + nullable** (`shelfLife: ShelfLife | null`), not optional.
2. **Error codes.** New codes go in the context's `<Name>ErrorCode` union (`<context>.<kebab>`), common ones in `CommonErrorCode`, validator codes in `ValidationCode`. Every code the backend can send must be here and vice versa: `ErrorCatalogueTests` (architecture tests) fails both ways.
3. **Route.** Add the op to `routes.tsp` with `@route`, `@operationId` (camelCase, the same `OperationId` as the Wolverine endpoint) and its template.
4. **Compile and regenerate**: `pnpm nx run @cooquoi/api-client:generate` (runs `@cooquoi/contracts:compile` first). Commit `contracts/generated/openapi.yaml` and the client.
5. **Backend** (`dotnet-feature`): the endpoint calls the matching `OperationContract.{List,OwnResource,Read,Create,ReplaceAction}` in its Wolverine `Configure(HttpChain)`. A string member with a closed set carries `[ContractCodes(typeof(Unit), nameof(Unit.All))]` or `[ContractEnum<IngredientStatus>]` (`dotnet/src/Cooquoi.Platform/OpenApi/ContractValuesAttribute.cs`); without it the drift gate reports a missing `enum`.
6. **Drift.** `dotnet build dotnet/Cooquoi.slnx` regenerates `dotnet/generated/openapi.json` (incremental: it reruns when the API assembly changes, not when only the JSON is deleted). Then `pnpm nx run @cooquoi/contracts:drift` (Docker `oasdiff` piped to `contracts/scripts/drift-gate.mjs`): it prints `drift: none (accepted: …)` or one line per change. Fix the contract or the API until it is clean; never add to `accepted` in the gate except for a document-level difference that cannot be fixed, with its reason. CI job `contract-drift` also fails when either generated file is not committed.
7. **Catalogue test**: `dotnet test --project dotnet/tests/Cooquoi.Architecture.Tests`.
8. **Webapp**: `api-client-sync` skill (queries, actions, view models, MSW fixtures; remove what the contract dropped).

## Pitfalls

- `Page<T>` emits inline; add `@friendlyName` only if a named schema is needed.
- Things that are only in doc comments, not checked: `Accept-Language`, length limits on query strings, sort keys. Keep them accurate by hand.
- The API still reads numbers from JSON strings (web defaults); the document hides it. Do not rely on it in clients.
- Doc text (`description`, `summary`, `examples`, `tags`, `title`) is excluded from the drift check.
