---
name: webapp-review
description: Review a change to the Cooquoi Next.js webapp against its conventions — structure and boundaries, thin routes, clean-code limits, Astryx and tokens, i18n, server/client boundary, accessibility, tests and refactor report. Use when reviewing webapp code written by a person or an agent, and as a self-check at the end of webapp-feature.
---

# Webapp review

Conventions: `ts/apps/cooquoi-webapp/AGENTS.md`. Report findings as a list: **file:line — problem — expected fix**, most severe first. Do not approve with open findings.

## Checklist

### Structure and boundaries
- [ ] Each file sits in the right type folder with the right suffix (`*.query.ts`, `*.action.ts`, `*.schema.ts`, `*.view-model.ts`, component folder).
- [ ] `app/` imports only feature barrels and `@/shared/*`.
- [ ] No feature imports another feature; `shared/` imports no feature.
- [ ] Barrel exports only what `app/` uses.

### Routes and data
- [ ] Pages and layouts are thin (≈30 lines): params → query → component.
- [ ] Reads go through `*.query.ts` (`server-only`, `cache()`); writes through Server Actions with `defineAction`.
- [ ] Route handlers only for auth callback/logout or cached GET.
- [ ] No business rule in the webapp (only display rules in view models).
- [ ] API types come from the generated client; no hand-written copies.

### Clean code
- [ ] Files ≤ ~150 lines, functions ≤ ~30 lines, ≤ 2 parameters (else a typed object).
- [ ] Names express intent; functions start with a verb; booleans with `is`/`has`/`can`.
- [ ] No `any`, no commented-out code, no dead code (`knip` clean).
- [ ] Named exports only, except Next.js special files.

### UI
- [ ] Astryx components or compositions of them.
- [ ] No px, hex, Tailwind or custom shadows; tokens and CSS Modules only.
- [ ] Responsive (intrinsic grids, theme breakpoints), light and dark mode safe.
- [ ] `'use client'` only on interactive leaves.
- [ ] Accessibility: labelled fields, named icon buttons, ordered headings.

### i18n
- [ ] No literal user-facing strings.
- [ ] New keys exist in `en`, `fr` and `vi`.
- [ ] Dates, numbers and money formatted with next-intl / `Intl`.
- [ ] Error messages come from `ActionResult.error.code`.

### Tests
- [ ] Every new query, action, view model and component has tests (per `webapp-test`).
- [ ] Production code was not changed for testability.
- [ ] API calls mocked with MSW (not `vi.mock` of the client, not a hand-stubbed `fetch`); `vi.mock` only for session and Next.js modules.
- [ ] Assertions were not weakened.

### Refactor report
- [ ] A refactor report exists (`refactor` skill template).
- [ ] Every skipped smell has an allowed reason (1–4).
- [ ] The verification loop ran (dead code, pass-throughs, speculative layers).

### Checks
- [ ] `lint`, `typecheck`, `test` on `@cooquoi/webapp`, `pnpm biome check` and `pnpm knip` pass — results shown, not assumed.
