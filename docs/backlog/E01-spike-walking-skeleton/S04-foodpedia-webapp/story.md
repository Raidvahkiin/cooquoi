# E01.S04 — Foodpedia webapp (phase 4)

## Goal

Foodpedia ingredient search in the **user app** (`@cooquoi/webapp`), on Astryx with the `cooquoi` theme and next-intl in `en` / `fr` / `vi`. Curator commands (C5 define-conversion) go to the separate curator app, story [E01.S06](../S06-curator-app/story.md).

## Acceptance

- [x] Q1 search works in three languages (Astryx table, paging, sort)
- [x] Astryx `AppShell` with the signed-in user's name (left over from S01)
- [x] Locale from the user profile (`GET /v1/me`), then `Accept-Language`, then `en`
- [x] Every `CommonErrorCode`, `FoodpediaErrorCode` and `ValidationCode` has an `en` / `fr` / `vi` message; the error catalogue test fails when one is missing; a failing search shows its error translated by code
- [x] Tests: Vitest Node (queries, view models + MSW) and browser mode (components)
- [x] Spike answers recorded: Astryx in Server Components, theme build, dark mode; paprika vs error red; Vitest browser mode + MSW

## Context

- [Spike plan, Phase 4](../../../architecture/spike-walking-skeleton.md) (lines 145-160) and Phase 1 "Not done" (line 50: AppShell, browser-mode project)
- [Webapp AGENTS.md](../../../../ts/apps/cooquoi-webapp/AGENTS.md) "Structure", "File types", "Testing"
- [ADR 0008 Decision](../../../architecture/adr/0008-web-ui-astryx.md); [ADR 0012 Decision](../../../architecture/adr/0012-internationalization.md) §1, §2, §5, §8
- [Foodpedia use cases](../../../contexts/foodpedia/use-cases.md) Q1 (lines 6-18)
- Generated client `ts/libs/api-client/src/generated/{sdk,types}.gen.ts` (S03 contract, used as is; a contract change goes through the `contract` skill)
- Starting point: the webapp has only auth routes and `features/household` (`getMe`); no next-intl, Astryx or browser-mode tests yet

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-i18n-locale-from-profile.md) | next-intl with the locale from the user profile | done | — | no | s04-t01 |
| [T02](T02-astryx-theme-appshell.md) | Astryx `cooquoi` theme, fonts, AppShell, browser-mode tests | done | T01 | no | s04-t02 (subagent) |
| [T03](T03-error-display-by-code.md) | Error messages and display by code | done | T02 | no | s04-t03 (subagent) |
| [T04](T04-error-translation-catalogue.md) | Error catalogue: every contract code has a translation | done | T03 | yes | s04-t04 (subagent) |
| [T05](T05-ingredient-search.md) | Foodpedia ingredient search page (Q1) | done | T03 | yes | s04-t05 (subagent) |
| [T06](T06-define-conversion.md) | Ingredient page with the define-conversion form | dropped (moved to E01.S06) | — | — | — |
| [T07](T07-verify-and-findings.md) | Dev users, end-to-end check under Aspire, phase 4 findings | done | T04, T05 | no | s04-t07 (subagent) |

T01 → T02 → T03 are sequential (same `messages/*.json`, layout and test setup). T04 (`dotnet/tests` only) can run alongside T05 (webapp only). Review (`webapp-review` + the T04 `dotnet-review` scope) runs at story close, by `backlog-run`.

Decided at planning (2026-11-22): curator screens live in a separate curator app (E01.S06), not behind a role check in the user app; Playwright e2e is a later story after E01; fr/vi dev users are seeded in T07.

## Summary

Built in the user app: next-intl with the profile locale, Astryx `cooquoi` theme and AppShell, errors translated by code, a .NET catalogue test, and Q1 search at `/foodpedia`. T06 moved to E01.S06. Reviews and the check under Aspire led to fixes: an API call timeout, a translated error page and a shared `/v1/me` call.
Verified: webapp tests (64) and build are green, architecture tests 15/15. Under Aspire, search works in three languages, light and dark screenshots were taken, and a down API gives the error page in ~5 s (be08c57).
The UI is a spike (table, paging and sort). The real screen is mobile-first with a card list. Open decisions (HTTPS in dev, session cookie size, dev password), the review findings and the S05 items: [findings.md](findings.md).
