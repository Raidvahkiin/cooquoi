---
name: webapp-feature
description: Implement a screen, feature or slice in the Cooquoi Next.js webapp (queries, Server Actions, form schemas, view models, components, i18n keys, tests) following the webapp conventions. Use when asked to add or change webapp functionality.
---

# Webapp feature

Conventions: `ts/apps/cooquoi-webapp/AGENTS.md`. Read it first.

## Steps

1. **Understand the slice.** Identify the feature folder (`src/features/<feature>/`, one per bounded context), the API operations involved (generated `@cooquoi/api-client`), and the screens. If an operation is missing from the client, stop: the contract must change first (then run `api-client-sync`).
2. **View models** — `models/<name>.view-model.ts`: the type the screen needs and a pure `to<Name>ViewModel(dto)`. Display rules only.
3. **Queries** — `queries/<name>.query.ts`:
   ```ts
   import 'server-only';
   import { cache } from 'react';
   import { notFound } from 'next/navigation';
   import { createApiClient } from '@/shared/api';
   import { toPantryViewModel } from '../models/pantry.view-model';

   export const getPantry = cache(async (pantryId: string) => {
     const api = await createApiClient();
     const result = await api.pantry.readPantry({ path: { pantryId } });
     if (result.response.status === 404) notFound();
     return toPantryViewModel(result.data);
   });
   ```
4. **Schemas** — `schemas/<name>.schema.ts`: zod schema of each form's input.
5. **Actions** — `actions/<name>.action.ts`: `'use server'`, one action per file, `defineAction(schema, handler)`, revalidate what changed, return `ActionResult`.
6. **Components** — `components/<name>/<name>.tsx`, following the `webapp-ui` skill. Server Components by default; `'use client'` only on interactive leaves. Forms validate with the same zod schema and display `ActionResult` errors by translated `code`.
7. **Barrel** — export from `index.ts` only what `app/` needs (queries, actions used by pages, screen components).
8. **Route** — thin `page.tsx` / `layout.tsx` in `src/app/` (≈30 lines), plus `loading.tsx` / `error.tsx` / `not-found.tsx` where relevant.
9. **i18n** — add keys under the feature namespace in `messages/en.json`, `fr.json` and `vi.json`. No literal strings.
10. **Tests** — run the `webapp-test` skill for every new query, action, view model and component.
11. **Refactor** — run the `refactor` skill (level 0–2, verification loop).
12. **Self-review** — go through the `webapp-review` checklist and fix every finding.

## Done when

- `pnpm nx run @cooquoi/webapp:lint`, `:typecheck`, `:test` and `pnpm knip` pass.
- The refactor report is included in the final message.
- No business rule was implemented in the webapp (if one seemed needed, it belongs in the API — report it).
