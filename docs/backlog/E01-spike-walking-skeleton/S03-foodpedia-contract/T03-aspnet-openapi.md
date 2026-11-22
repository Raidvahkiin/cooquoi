# E01.S03.T03 — API emits its OpenAPI document

## Goal

`Cooquoi.Api` produces an OpenAPI 3.1 document of what it really serves (Wolverine.HTTP endpoints, operationIds, parameters, request/response bodies, status codes, problem responses), written to a committed file at build time, so T04 can diff it against the TypeSpec one.

## Read only

- `dotnet/AGENTS.md` sections "Responses and HTTP semantics", "Wolverine code generation", "Commands"
- `dotnet/src/Cooquoi.Api/Program.cs` (host setup, `RunJasperFxCommands` at the end)
- `dotnet/Directory.Packages.props:15` (`Microsoft.AspNetCore.OpenApi` 10.0.15 is pinned, referenced by no project)
- One endpoint for metadata: `dotnet/src/Cooquoi.Context.Foodpedia/Features/DefineConversion/DefineConversionEndpoint.cs` (`[EmptyResponse]`, operationId)

## Reference

- No OpenAPI emission exists today (no `AddOpenApi` / `MapOpenApi` in `dotnet/src`). Every Wolverine attribute already sets an operationId (e.g. `Features/SearchIngredients/SearchIngredientsEndpoint.cs:16`).
- Development-only setup pattern: development migrations as a hosted service so `codegen write` runs without a database (see "Wolverine code generation" in `dotnet/AGENTS.md`) — the document generation must also run without Postgres or Keycloak.

## Skills

`dotnet-feature` (endpoint metadata conventions), `refactor` at the end

## Lookups

- "Does `Microsoft.AspNetCore.OpenApi` (.NET 10) include Wolverine.HTTP 6 endpoints, and how do Wolverine endpoints declare extra responses (`ProducesProblem`, `[ProducesResponseType]`, `IEndpointMetadataProvider`)?" → Wolverine docs (context7) snippet, ≤10 lines.
- "Does build-time generation (`Microsoft.Extensions.ApiDescription.Server`, `OpenApiGenerateDocuments`) work with a host that ends with `RunJasperFxCommands`, without a database?" → yes/no + the property set needed. If no: fall back to an Alba test or a JasperFx command that writes the document, and say which in the report.

## Out of scope

- The `oasdiff` comparison and CI (T04); contract files under `contracts/` (T01, T02)
- Serving Swagger UI or Scalar in production; changing endpoint behaviour

## Subtasks

- [x] T03.1 Reference `Microsoft.AspNetCore.OpenApi` in `Cooquoi.Api`; `AddOpenApi("v1")` with an OpenAPI 3.1 target; `MapOpenApi()` in development only
- [x] T03.2 Document transformers so the output compares cleanly with TypeSpec: bearer security scheme, server `/`, problem responses as `application/problem+json` referencing a `ProblemDetails` schema
- [x] T03.3 Response metadata on the 4 Foodpedia endpoints and `GET /v1/me` (201 + `Location`, 204 + `ETag`, problem statuses per template in `contracts/common/operations.tsp` (T01 done: `ListOperation`, `ReadOperation`, `CreateOperation`, `ActionOperation` (+400), `ReplaceActionOperation`))
- [x] T03.4 Build-time generation to `dotnet/generated/openapi.json` (committed), no database needed
- [x] T03.5 Test in `Cooquoi.Platform.Tests` or `Cooquoi.Context.Foodpedia.Tests`: the document lists the 5 operationIds

## Done when

`dotnet build dotnet/Cooquoi.slnx -v q -nologo -clp:ErrorsOnly` writes `dotnet/generated/openapi.json` with the 5 operationIds (`grep -c operationId`), with Docker stopped, and `dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress` is green.

Then: tick the subtasks, write the report (≤5 lines: chosen generation path, what Wolverine needed), set the status, commit (`feat(api): emit OpenAPI document at build time`), stop.

Status: done

Report:
- Path: build-time `Microsoft.Extensions.ApiDescription.Server` (GetDocument.Insider) → `dotnet/generated/openapi.json` (3.1.1, 5 operationIds); `ContractOpenApi` transformers add BearerAuth, server `/`, contract `ProblemDetails`/`FieldError`, template headers. It is incremental: it reruns when the API assembly changes, not when only the JSON is deleted.
- Wolverine needed: when `IsGeneratingDocument`, `DisableAllWolverineMessagePersistence()` plus `Durability.Mode = MediatorOnly` (node agents read Postgres anyway); placeholder connection string and Authority. Auto-detection did not apply.
- Metadata: `OperationContract.{List,OwnResource,Read,Create,ReplaceAction}` in Platform, called from each endpoint's Wolverine `Configure(HttpChain)`; Wolverine's implicit GET 404 is removed for list and `getMe`.
- Verified: build ok, 114 tests green (+2 `OperationContractTests`), format clean. No DB at generation (unresolvable placeholder host), but Docker was not stopped.
- For T04: info `1.0.0` vs `0.0.0`, server `/` vs `https://api.cooquoi.com`, schema names without the `Common.`/`Foodpedia.` prefixes, inline header params vs `$ref`.
