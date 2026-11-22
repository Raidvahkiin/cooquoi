# E01.S04.T02 — Astryx `cooquoi` theme, fonts, AppShell and browser-mode tests

## Goal

Every signed-in page renders inside an Astryx `AppShell` showing the signed-in user's name, styled by the built `cooquoi` theme (warm + paprika) with light/dark mode and a Vietnamese-capable font; the first component is tested in Vitest browser mode (carry-overs from S01).

## Read only

- `ts/apps/cooquoi-webapp/AGENTS.md` sections "Structure", "File types", "UI, styling, text", "Testing"
- [ADR 0008](../../../architecture/adr/0008-web-ui-astryx.md) **Decision**; [ADR 0012](../../../architecture/adr/0012-internationalization.md) **Decision** §8 (fonts)
- `.agents/skills/webapp-ui/SKILL.md` (Astryx, tokens, CSS Modules, theme CLI)
- `ts/apps/cooquoi-webapp/src/app/layout.tsx`, `src/app/page.tsx`, `src/app/account/page.tsx` (as left by T01)
- `ts/apps/cooquoi-webapp/vite.config.mts`, `test/setup.ts`

## Reference

- No Astryx code exists yet: follow the Astryx docs (Lookups) and `webapp-ui` skill; the target layout is in `AGENTS.md` "Structure": `app/(app)/layout.tsx` = `requireSession()` + AppShell, `app/layout.tsx` = theme, i18n provider, fonts
- User name: `getMe()` (`src/features/household/queries/get-me.query.ts:8`, `MeViewModel.displayName`)
- Logout link target: `src/app/auth/logout/route.ts`
- Vitest config: single Node project at `vite.config.mts:13-26`; turn it into `test.projects` = `node` (`src/**/*.test.ts`, current setup) + `browser` (`src/**/*.test.tsx`, Chromium via Playwright, its own setup)

## Skills

`webapp-ui`, `webapp-feature`, `webapp-test`

## Model

opus (new structure: theme build, layout, test project; no pattern to copy)

## Lookups

- "Astryx (`@astryxdesign/core`): install, theme CLI to build a theme from seeds, how the theme and its CSS are loaded in Next.js App Router, which components are Server-Component safe, dark mode switch, `AppShell` props" → context7 docs tool or the Astryx agent docs/CLI, ≤10 lines each.
- "Vitest 5 browser mode with Playwright provider: package names and `test.projects` config" → context7 (`vitest`), ≤10 lines.

## Decisions (settled at planning)

- Dependencies go through the `catalog:` of `pnpm-workspace.yaml`. Playwright's Chromium is installed with the Playwright CLI, not committed.
- Theme seeds: warm neutrals + paprika primary; theme sources and build script committed under the webapp (path of your choice, named in the report). No px, no hex in CSS Modules (tokens only); hex is allowed only in theme seeds.
- Font: `next/font` with the `vietnamese` subset; if the Astryx default (Figtree) lacks Vietnamese, use Be Vietnam Pro or Inter in the theme.
- Move `src/app/page.tsx` and `src/app/account/page.tsx` under `src/app/(app)/`; `(app)/layout.tsx` (≈30 lines) calls `requireSession()` (or `requireAccessToken()`), `getMe()` and renders `AppHeader`/shell from `src/shared/ui/` or the `household` feature (a composite showing a user's name belongs to the feature that owns `Me`: `features/household/components/user-menu/`). AppShell nav entry for Foodpedia (`/foodpedia`) added now, page comes in T05.
- Shell labels (nav, sign out) are translated in `en` / `fr` / `vi`.

## Out of scope

- Error messages (T03), Foodpedia search (T05), curator app (E01.S06), Biome/knip rules (S05), Playwright e2e tests

## Subtasks

- [x] T02.1 Install Astryx, build the `cooquoi` theme (light + dark), load it and the font in `src/app/layout.tsx`
- [x] T02.2 Route group `(app)` with `layout.tsx` + AppShell; move the two pages
- [x] T02.3 User menu component (name + sign out), translated
- [x] T02.4 Vitest `projects`: `node` + `browser` (Chromium); render helper `test/render.tsx` wrapping `NextIntlClientProvider` with `messages/en.json`
- [x] T02.5 Browser test of the user menu: name visible, sign-out link by role and accessible name, French labels with `fr` messages
- [x] T02.6 Check paprika vs error red in light and dark (contrast of Astryx error colour vs primary); write the spike answers (Astryx in Server Components, theme build, dark mode, paprika vs red, browser mode setup) as 3–6 lines in the report for T07

## Done when

`pnpm nx run-many -t typecheck lint test -p @cooquoi/webapp 2>&1 | tail -15` is green with both Vitest projects running, and `pnpm nx run @cooquoi/webapp:build 2>&1 | tail -5` succeeds.

Then: tick the subtasks, write the report (≤5 lines + the spike answers), set the status, commit (`feat(webapp): Astryx cooquoi theme and AppShell (E01.S04.T02)`), stop.

Status: done

Report:
- Changed: Astryx 0.7.17 + StyleX + Vitest browser deps (catalog); `cooquoi` theme source `src/shared/ui/theme/cooquoi.theme.ts`, built outputs `cooquoi.{css,js,d.ts}` committed, rebuilt by `pnpm nx run @cooquoi/webapp:theme-build`; `shared/ui` (`UiProvider`, `AppFrame`, `bodyFont`); `app/(app)/layout.tsx` + moved pages; `features/household/components/user-menu`; `common.nav.*` keys in en/fr/vi.
- Verified: typecheck, lint, test (28 node + 3 browser) green; build green, Vietnamese font subset in the CSS. Not looked at in a running app (no stack started).
- Surprising: Astryx postinstall only nudges `astryx init` (`allowBuilds: false`); `/auth/*` links render a plain `<a>` so next/link never prefetches logout; heading weights need a type scale (dropped); pre-existing Biome organizeImports errors in 3 untouched files left for S05.

Spike answers (for T07):
- Server Components: every Astryx entry ships `'use client'` inside, so layouts, pages and composites import `@astryxdesign/core/<Name>` directly with serializable props and ReactNode slots; only `Theme` and `LinkProvider` (function prop) need a client wrapper (`shared/ui/ui-provider`).
- Theme build: `defineTheme` + `themePalette({ accent, neutral, danger, recipe: 'astryx-oklch-v2' })`, `astryx theme build` writes CSS (177 KB) + JS + d.ts; CSS order reset → astryx → cooquoi. Font through the token `var(--font-be-vietnam-pro)` set by `next/font` (Figtree has no Vietnamese).
- Dark mode: tokens compile to `light-dark()`; `<Theme mode="system">` follows the OS; a user toggle later only switches `mode`.
- Paprika vs error red: default error (#E3193B) sat 13° of hue / ΔE(OK) 8 from paprika → too close. Theme now seeds accent #B9531F (hue 44°) and danger #C2185B (crimson, hue 6°): ΔE 13 light / 20 dark; accent on white 4.9:1, text-error 7.0 / 8.6:1, text-accent 6.9 / 7.5:1. Dark text-accent vs text-error stay close (ΔE 9): errors never rely on colour alone (Banner icon + title).
- Vitest browser mode: `test.projects` `node` + `browser` (`@vitest/browser-playwright` `playwright()`, `instances: [{ browser: 'chromium' }]`), `vitest-browser-react` `render` + `expect.element`; needs `oxc: { jsx: { runtime: 'automatic' } }` (tsconfig keeps `jsx: preserve`) and `test/**/*.tsx` in tsconfig; Chromium via `pnpm exec playwright install chromium`. MSW is not wired in the browser project (components take view models as props); when a client component fetches, add `msw/browser` `setupWorker` with `mockServiceWorker.js` from a test-only `publicDir`, not the app `public/`.
- T03 `ErrorMessage`: Astryx `Banner` (`@astryxdesign/core/Banner`, `status="error"`, `title`, `description`, `collapsible={false}`).
