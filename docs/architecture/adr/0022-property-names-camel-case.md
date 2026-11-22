# ADR 0022 — Property names are camelCase outside the process

- **Status**: Accepted
- **Date**: 2026-11-22

## Context

The repo serves several platforms and languages: a C# backend, a TypeScript webapp and generated client, the TypeSpec contract, and Flutter later. [ADR 0021](0021-enum-values-camel-case.md) gave enum values one spelling. Property names need the same rule, or each client and each store reader learns a different spelling. C# code keeps PascalCase members, which is the idiom of that language. Only what is serialized must agree.

An audit before E02.S02 found every serialized property name already camelCase, but the rule came from framework defaults, not from a decision:

| Place | Casing | Decided by |
|---|---|---|
| HTTP JSON (requests, responses, `Paged<T>`, `errors[]`) | camelCase | ASP.NET `JsonOptions` web defaults, read by Wolverine.HTTP |
| `ProblemDetails` extensions (`code`, `params`, `traceId`) and `params` keys | camelCase | Literal keys in `ProblemWriter`, domain errors and `ValidationCodes` (dictionary keys skip the naming policy) |
| Contract and both OpenAPI documents | camelCase | TypeSpec models; the drift check keeps the C# document equal |
| jsonb columns (`ingredients.conversions`) | camelCase | `JsonSerializerDefaults.Web` in `ConversionTableJson` |
| Wolverine envelope bodies (outbox, inbox, dead letters) | camelCase | Wolverine's default System.Text.Json serializer |
| Seed files | camelCase | Written so; read with `JsonSerializerDefaults.Web` |
| Idempotency stored responses | as sent | Raw response bytes, replayed unchanged |
| Webapp, generated client, `messages/*.json` keys | camelCase | Generated from the contract; i18n paths |

| Option | Verdict |
|---|---|
| **camelCase for every serialized property name, guarded by tests, no new configuration** | **Chosen**: already true everywhere, and a test now fails on the first exception |
| Pin each serializer explicitly (`UseSystemTextJsonForSerialization`, `ConfigureHttpJsonOptions`) | Rejected for now: it repeats the defaults. If a default changes, the tests below fail, and that is the time to pin it |
| `[JsonPropertyName]` per property | Rejected: one more spelling to keep in step, per property |

## Decision

- Every property name that leaves the process is **camelCase**: HTTP bodies, `ProblemDetails` extensions and their `params` keys, the contract, jsonb columns, Wolverine message bodies, seed files and integration events. C# members stay PascalCase. The serializer converts them.
- Our own `JsonSerializerOptions` start from `JsonSerializerDefaults.Web`. No `[JsonPropertyName]`. Dictionary keys we write (`params`) are written camelCase, because the naming policy does not touch them.
- Guards: `PropertyNameRulesTests` (architecture) checks the contract's properties, the seed keys, that every `JsonSerializerOptions` starts from web defaults and every `JsonSerializer` call passes options, and the absence of `[JsonPropertyName]`. `PropertyNameTests` (Platform) serializes through the host's HTTP and Wolverine serializers. `ConversionTableJsonTests` (Foodpedia) reads the jsonb keys with SQL. `params` keys are literals in code, checked by the endpoint tests that assert each problem's `params`.
- **Exceptions**, which are not our serialization: Postgres table and column names stay snake_case (`UseSnakeCaseNamingConvention`), because unquoted SQL identifiers fold to lower case. HTTP header names (`ETag`, `Location`), OAuth/OIDC fields (`access_token`) and Keycloak's own files keep the format their standard defines.

## Consequences

- One rule for every client: a contract property is exactly the JSON key that TypeScript, and later Dart, reads and writes.
- A SQL reader of a jsonb column sees the same keys as the API. Renaming a C# property that is stored as jsonb, or that is part of a persisted message, renames its key: it needs a data migration, and the guard tests do not catch it.
- No data migration was needed: no stored key changed.
