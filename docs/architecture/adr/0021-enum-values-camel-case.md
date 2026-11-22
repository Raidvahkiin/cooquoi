# ADR 0021 — Enum values are camelCase everywhere

- **Status**: Accepted
- **Date**: 2026-11-22

## Context

E02.S01 added the first multi-word enum value, `ExpiryKind.LongLife`, and showed that it had three spellings: `LongLife` in the database (`HasConversion<string>()`), `long-life` in the contract, the seed and the API (`ContractEnumNames`), and the C# member name. Each layer converts differently, and a mismatch only shows on the first multi-word member. The webapp is TypeScript, where camelCase is the idiom for identifiers and union members.

| Option | Verdict |
|---|---|
| **Plain C# enums, one camelCase code (`longLife`) in the contract, API, seed and database** | **Chosen**: one spelling everywhere, one conversion function, no new library |
| Smart enums (a class per enum with `Code` and `Parse`) | Rejected: more code per enum, plus EF, JSON and OpenAPI adapters, for a list of fixed values that has no behaviour |
| Keep kebab in the API and PascalCase in the database | Rejected: two conversions per value, and the database spelling differs from what every other layer reads |

## Decision

- An enum value's **code** is the C# member name in camelCase: `LongLife` → `longLife`, `Active` → `active`. It is the same in the TypeSpec union, the JSON of every request and response, the seed files, the published API and integration events of a context, and the database column.
- One function converts between the member and the code: `ContractEnumNames.ToCode` and `Parse` (`Cooquoi.Platform`). `ContractEnumAttribute<TEnum>` lists the codes for validation and the drift check. EF Core stores enums through one camelCase value converter applied by convention, not a per-property `HasConversion<string>()`.
- Existing rows are converted by a migration in each context that stores enums.
- Unchanged: routes and actions (`:set-expiry-kind`) and error codes (`foodpedia.ingredient-retired`) stay kebab-case. They are URL segments and identifiers, not enum values.

## Consequences

- One rule for the frontend: a contract union value is exactly what TypeScript receives and sends.
- A database read without the application (SQL, reports) sees the same codes as the API.
- Renaming an enum member renames its code: that is a contract change and needs a data migration.
