---
name: api-client-sync
description: Update the Cooquoi webapp after a TypeSpec API contract change — regenerate the TypeScript client, fix queries, actions, view models and MSW fixtures, and remove what the contract dropped. Use whenever files under contracts/ change or the generated client is out of date.
---

# API client sync

Decision: [ADR 0009](../../../docs/architecture/adr/0009-api-contract-typespec.md). TypeSpec is the source of truth; the client is generated, never edited by hand.

## Steps

1. **Compile the contract and regenerate the client** through Nx (TypeSpec → `contracts/generated/openapi.yaml` → Hey API client in `ts/libs/api-client`). Use the project targets; check them with `pnpm nx show project <name>` rather than guessing.
2. **Read the contract diff** (`git diff contracts/`): list added, changed and removed operations and models.
3. **Find breakage**: `pnpm nx run @cooquoi/webapp:typecheck`. Every error points to a query, action, view model, fixture or test to update.
4. **Update in this order**:
   1. Fixtures (`test/fixtures/*`) — typed with the new API types.
   2. View models — mapping functions follow the new DTOs.
   3. Queries and actions — new parameters, paths, responses.
   4. MSW handlers — new paths and payloads.
   5. Components — only if the view model changed shape.
5. **Removed from the contract → removed from the webapp.** Delete queries, actions, schemas, view-model fields, fixtures, handlers, i18n keys and components that only served removed operations or fields.
6. **Run the `refactor` skill.** Contract changes are the main source of dead code: the verification loop is mandatory.
7. **Run checks**: `lint`, `typecheck`, `test` on `@cooquoi/webapp` and `pnpm knip`.

## Rules

- Never hand-edit generated files, never add local copies of API types to silence errors.
- If the webapp needs something the contract does not offer, stop and propose a contract change; do not work around it in the webapp.
- Keep each change mechanical and reviewable: one commit for the regenerated client, one for the webapp adaptation.
