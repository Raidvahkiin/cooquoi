# E01.S04.T04 — Error catalogue: every contract code has a translation

## Goal

The error catalogue architecture test also fails when a `…ErrorCode` or `ValidationCode` value of the contract has no message in one of the webapp catalogues `messages/en.json`, `fr.json`, `vi.json` (or a message exists for a code the contract no longer declares).

## Read only

- `dotnet/AGENTS.md` sections "Tests", "Commands"
- `dotnet/tests/Cooquoi.Architecture.Tests/ErrorCatalogue.cs` (contract reader at :32-44) and `ErrorCatalogueTests.cs` (assertion helper `ShouldMatch` at :17)
- `dotnet/tests/Cooquoi.Architecture.Tests/CooquoiArchitecture.cs:37-45` (`SolutionDirectory`)
- `ts/apps/cooquoi-webapp/messages/en.json` `errors` namespace only (written by T03)

## Reference

- Contract codes: reuse the existing reader in `ErrorCatalogue.cs:32-44` (YamlDotNet over `contracts/generated/openapi.yaml`, enum schemas ending in `ErrorCode`, plus `ValidationCode`)
- Webapp path: `Path.Combine(CooquoiArchitecture.SolutionDirectory, "..", "ts", "apps", "cooquoi-webapp", "messages", $"{locale}.json")`
- Key layout (T03 decision): `errors` is nested by the dots of the code; flatten with `System.Text.Json` (`JsonDocument`), joining property names with `.` down to string leaves, under `errors`
- Set comparison and message style: `ErrorCatalogueTests.cs:17`

## Skills

`dotnet-test`

## Model

sonnet

## Out of scope

- Webapp code (T03); CI wiring of the test (S05, it already runs with the architecture tests)

## Subtasks

- [x] T04.1 `ErrorCatalogue.TranslatedCodes(locale)` reading and flattening `errors` of one catalogue
- [x] T04.2 `ErrorCatalogueTests`: `[Theory]` over `en`, `fr`, `vi`: contract codes (problem + validation) ⊆ translated and translated ⊆ contract, message naming the locale and missing codes
- [x] T04.3 Deliberate violation: delete one key from `messages/vi.json` locally → test fails naming `vi` and the code; revert

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Architecture.Tests -v q --no-progress 2>&1 | tail -5` is green on the clean tree and fails on the T04.3 violation.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`test(architecture): error codes translated in every locale (E01.S04.T04)`), stop.

Status: done

Report: ErrorCatalogue.TranslatedCodes(locale) + Theory over en/fr/vi (contract codes equal translated codes). Architecture tests 15/15 green; removing foodpedia.nameTaken from vi.json fails naming vi and the code (reverted). Nothing left.
