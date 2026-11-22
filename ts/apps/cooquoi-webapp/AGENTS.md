# Webapp (`@cooquoi/webapp`)

Next.js App Router webapp, backend-for-frontend of the C# API. Decisions: [ADR 0008 (Astryx)](../../../docs/architecture/adr/0008-web-ui-astryx.md), [0011 (auth/BFF)](../../../docs/architecture/adr/0011-webapp-api-auth.md), [0012 (i18n)](../../../docs/architecture/adr/0012-internationalization.md), [0013 (structure)](../../../docs/architecture/adr/0013-webapp-structure.md).

> **Rebuild in progress.** The old UI was removed; `src/` holds code that follows this document (auth, household, i18n, Astryx theme and AppShell, Foodpedia search). Further screens are rebuilt with Astryx.

**Business logic lives in the API, never here.** The webapp loads data, sends actions, maps API data for screens, validates forms and renders UI.

## Skills

| Task | Skill |
|---|---|
| Implement a screen or slice | `webapp-feature` |
| Build or change UI | `webapp-ui` |
| Write or fix tests | `webapp-test` |
| After a contract change | `api-client-sync` |
| Review a change | `webapp-review` |
| After any change | `refactor` |

## Structure

```
src/
├─ app/                       # ROUTING ONLY
│  ├─ (public)/login/page.tsx
│  ├─ (app)/layout.tsx        # requireSession(), AppShell
│  ├─ (app)/<route>/page.tsx
│  ├─ auth/callback/route.ts  # OIDC callback
│  ├─ auth/logout/route.ts
│  ├─ manifest.ts             # PWA
│  └─ layout.tsx              # Theme, i18n provider, fonts
├─ features/<feature>/
│  ├─ index.ts                # public API — the only import path for app/
│  ├─ queries/<name>.query.ts
│  ├─ actions/<name>.action.ts
│  ├─ schemas/<name>.schema.ts
│  ├─ models/<name>.view-model.ts
│  ├─ components/<name>/<name>.tsx (+ .module.css, .test.tsx)
│  └─ hooks/use-<name>.ts
└─ shared/
   ├─ api/        # createApiClient(), toActionError()
   ├─ auth/       # oidc client, iron-session, getSession(), requireSession()
   ├─ actions/    # ActionResult, defineAction()
   ├─ i18n/       # next-intl request config
   ├─ config/     # env parsed with zod
   └─ ui/         # webapp-wide composites on Astryx
messages/en.json fr.json vi.json   # one top-level namespace per feature
test/                              # msw server, handlers, fixtures, render helpers
```

### Dependency rules (enforced by Biome `noRestrictedImports`)

- `app/` imports only `@/features/<x>` (barrel) and `@/shared/*`.
- A feature never imports another feature. Pages compose features.
- `shared/` never imports a feature.
- API types come from `@cooquoi/api-client` (generated). Never re-declare them.

## File types

| Type | Rules |
|---|---|
| `page.tsx` / `layout.tsx` | ≈30 lines max. Read params → call query → render feature component. No logic, no `fetch`. |
| `*.query.ts` | `import 'server-only'`. One exported function, wrapped in `cache()`. Calls the API client, returns a view model. 404 → `notFound()`. |
| `*.action.ts` | `'use server'`. One exported action, built with `defineAction(schema, handler)`. Validate → call API → `revalidatePath`/`revalidateTag` → `ActionResult`. Never throws for expected errors. |
| `*.schema.ts` | zod schema of the form input; used by the form (client) and the action (server). |
| `*.view-model.ts` | View model type + pure `toXxxViewModel(dto)`. Display rules only, never business rules. |
| Component | One per folder/file, named export in PascalCase. `'use client'` only on leaf components needing interaction. |
| `use-*.ts` | Client-only UI state (open/close, optimistic state). |
| `route.ts` | Only for auth callback/logout and cached GET endpoints. Never for mutations. |

### `ActionResult`

```ts
type ActionResult<T = void> =
  | { ok: true; data: T }
  | { ok: false; error: ApiError };
```

`ApiError = { code, params?, fieldErrors? }` is the type of `shared/api/to-api-error.ts`, shared with queries; `code` and `params` come from the API's `ProblemDetails` and the UI translates them (`ErrorMessage`).

## Examples

```tsx
// app/(app)/pantries/[pantryId]/page.tsx
import { getPantry, PantryScreen } from '@/features/pantry';

export default async function PantryPage({ params }: PageProps<'/pantries/[pantryId]'>) {
  const { pantryId } = await params;
  const pantry = await getPantry(pantryId);
  return <PantryScreen pantry={pantry} />;
}
```

```ts
// features/pantry/actions/consume-stock-item.action.ts
'use server';
import { revalidatePath } from 'next/cache';
import { defineAction } from '@/shared/actions';
import { createApiClient } from '@/shared/api';
import { consumeStockItemSchema } from '../schemas/consume-stock-item.schema';

export const consumeStockItem = defineAction(consumeStockItemSchema, async (input) => {
  const api = await createApiClient();
  await api.pantry.consumeStockItem({ path: input.path, body: input.quantity });
  revalidatePath(`/pantries/${input.path.pantryId}`);
});
```

## Clean code

| Rule | Limit |
|---|---|
| File length | ≤ ~150 lines |
| Function length | ≤ ~30 lines |
| Parameters | ≤ 2, otherwise one typed object |
| Exports | Named only; default only where Next.js requires it |
| File names | `kebab-case` with type suffix |
| Function names | Verb first: `getPantry`, `consumeStockItem`, `toStockItemViewModel` |
| Booleans | `is` / `has` / `can` prefix |
| Types | No `any`; no hand-written copies of API types |
| Comments | Why, not what; no commented-out code |
| Dead code | None — `knip` fails CI |

## UI, styling, text

- Astryx components and tokens only; CSS Modules with Astryx tokens for the rest. No px, no hex, no Tailwind. Details: `webapp-ui` skill.
- No literal user-facing strings: `useTranslations('<feature>')` / `getTranslations`. Keys added to `en`, `fr` and `vi`.
- Format dates, numbers and money with `Intl` / next-intl formatters.

## Testing

- **Never change production code to make it testable** (no extra parameters, no injected dependencies).
- **MSW** mocks the network (every API call through the real generated client). **`vi.mock`** mocks modules that are not the network (`@/shared/auth`, `next/cache`, `next/navigation`).
- Queries, actions, view models: Vitest in Node (`*.test.ts`).
- Components: Vitest browser mode in real Chromium (`*.test.tsx`), query by role and accessible name.
- Pages: covered by Playwright end-to-end tests, not unit tests.
- Details: `webapp-test` skill.

## Tooling

- **Biome v2**: formatting and linting (size limits, complexity, import cycles, feature boundaries, unused code).
- **knip**: unused files, exports and dependencies.
- **ESLint**: only `@nx/enforce-module-boundaries`.

## Commands

```bash
pnpm nx run @cooquoi/webapp:dev
pnpm nx run @cooquoi/webapp:lint        # ESLint (Nx boundaries)
pnpm nx run @cooquoi/webapp:typecheck
pnpm nx run @cooquoi/webapp:test        # Vitest: node project (*.test.ts) + browser project (*.test.tsx)
pnpm nx run @cooquoi/webapp:theme-build # astryx theme build -> cooquoi.{css,js,d.ts} (committed)
pnpm biome check ts/apps/cooquoi-webapp
pnpm knip --workspace ts/apps/cooquoi-webapp
```

The browser project runs in real Chromium: install it once with `pnpm exec playwright install chromium`.

<!-- BEGIN:nextjs-agent-rules -->

## This is NOT the Next.js you know

This version has breaking changes — APIs, conventions, and file structure may all differ from your training data. Read the relevant guide in `node_modules/next/dist/docs/` (resolved from this file's directory; in monorepos the `next` package may not be visible from the repo root) before writing any code. Heed deprecation notices.

This block is written and re-added by `next dev` — verify at `node_modules/next/dist/server/lib/generate-agent-files.js`. Removing it from a diff only re-creates the uncommitted change; committing it with your work keeps the tree clean.

<!-- END:nextjs-agent-rules -->
