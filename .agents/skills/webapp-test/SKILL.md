---
name: webapp-test
description: Write or fix tests in the Cooquoi webapp — Vitest in Node with MSW for queries, actions and view models, Vitest browser mode for components, vi.mock for session and Next.js modules — without changing production code for testability. Use when adding tests, fixing failing tests, or as part of webapp-feature.
---

# Webapp tests

## Golden rule

**Never change production code to make it testable**: no extra parameters, no injected clients, no test-only exports or flags.

## Two mocking tools, two jobs

| Dependency | Tool |
|---|---|
| The C# API (through the generated client) — anything going over HTTP | **MSW**: intercepts the real request and answers with a fixture. The real client code runs. |
| Modules that are not the network: `@/shared/auth` (session), `next/cache`, `next/navigation` | **`vi.mock`** (Vitest built-in): replaces the module for the test file. |

Never `vi.mock` the generated API client and never stub `fetch` by hand: that skips request building, serialization and error mapping, which are what we want to test.

## What to test, how

| File type | Environment | Test style |
|---|---|---|
| `*.view-model.ts` | Node (`*.test.ts`) | Pure input → output. One test per display rule. |
| `*.query.ts` | Node | MSW returns a fixture; assert the view model, and `notFound()` on 404. |
| `*.action.ts` | Node | MSW captures the request; assert path, body and `ActionResult` (success, validation error, `ProblemDetails` → `error.code`). |
| `*.schema.ts` | Node | Only non-trivial rules (refinements, cross-field checks). |
| Components | **Browser mode** (`*.test.tsx`, real Chromium) | Render with `vitest-browser-react`, interact through the browser API (`page.getByRole(...)`, user events), assert visible outcome. |
| `page.tsx` / `layout.tsx` | — | Not unit-tested — Playwright end-to-end. |

## Setup (in `ts/apps/cooquoi-webapp/test/`)

- Vitest config with **two projects**: `node` (`**/*.test.ts`) and `browser` (`**/*.test.tsx`, Playwright provider, Chromium).
- `msw/handlers/<feature>.ts` — default handlers per feature, built from fixtures; shared by both projects.
- `msw/node.ts` — `setupServer(...handlers)` for the Node project (`onUnhandledFrame: 'error'`, MSW 3).
- `msw/browser.ts` — `setupWorker(...handlers)` for the browser project.
- `fixtures/<feature>.ts` — typed with API types from `@cooquoi/api-client`.
- `render.tsx` — renders with next-intl (English messages) and the Astryx `Theme`.
- `server-only` is aliased to an empty module in the Vitest config.

Check the current Vitest browser-mode and `vitest-browser-react` docs for exact imports before writing setup code.

## Session and Next.js modules

```ts
vi.mock('@/shared/auth', () => ({
  getSession: vi.fn().mockResolvedValue({ accessToken: 'test-token', locale: 'en' }),
}));
vi.mock('next/cache', () => ({ revalidatePath: vi.fn(), revalidateTag: vi.fn() }));
```

## Example — action (Node)

```ts
it('consumes a stock item', async () => {
  let body: unknown;
  server.use(
    http.post(`${API}/v1/pantries/:pantryId/items/:itemId\\:consume`, async ({ request }) => {
      body = await request.json();
      return HttpResponse.json(stockItemFixture);
    }),
  );

  const result = await consumeStockItem(validInput);

  expect(result).toEqual({ ok: true, data: undefined });
  expect(body).toEqual({ amount: 200, unit: 'g' });
});
```

## Rules

- One behaviour per test; name tests as sentences (`it('shows an empty state when the pantry has no items')`).
- Arrange with fixtures, override only what the test is about.
- Query elements by role and accessible name, never by CSS class or test id unless no role exists.
- No snapshot tests of whole components.
- No `any`, no `as unknown as` casts in tests — use typed fixtures.
- Fix a failing test by understanding the behaviour change; never weaken an assertion to make it pass.

## Done when

`pnpm nx run @cooquoi/webapp:test` passes and every new query, action, view model and component has tests.
