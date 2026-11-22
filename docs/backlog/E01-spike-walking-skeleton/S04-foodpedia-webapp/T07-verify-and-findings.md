# E01.S04.T07 — Dev users, end-to-end check under Aspire and phase 4 findings

## Goal

Dev users exist in `fr` and `vi`, the S04 part of the phase 4 "Done when" is observed in the running system (search in three languages, a translated error), and the spike answers and conventions learned are recorded.

## Read only

- [Spike plan](../../../architecture/spike-walking-skeleton.md) "Phase 4" (lines 145-160) and the Phase 1 "Status" block (lines 45-51) as the format for findings
- The reports of T01–T05 in this folder (`Report:` sections only)
- `aspire/keycloak/dev-users/cooquoi-users-0.json` (one user, 22 lines)
- `ts/apps/cooquoi-webapp/AGENTS.md` sections "Testing", "Commands"
- Root `AGENTS.md` "Tooling" for the Aspire command

## Reference

- Dev user shape: `aspire/keycloak/dev-users/cooquoi-users-0.json:4-19` (`attributes.locale`, password from `${COOQUOI_DEV_USER_PASSWORD}`, `realmRoles`). The profile locale is taken from the token's `locale` claim at first sign-in (`dotnet/src/Cooquoi.Context.Household/Features/GetMe/UserSignIn.cs:59`)
- Findings format: spike plan Phase 1 "**Status (…)**", "**Fixed by the tests**", "**Not done**" (lines 45-51)
- Run: `aspire run` from `aspire/Cooquoi.AppHost` in the background; browse the webapp with the built-in browser

## Skills

`run`

## Model

sonnet

## Out of scope

- Fixing anything larger than a few lines: record it as a finding or a new task instead; the curator app (E01.S06); Playwright e2e (later story) and CI (S05)

## Subtasks

- [x] T07.1 Add `dev-fr@cooquoi.local` (`locale: fr`) and `dev-vi@cooquoi.local` (`locale: vi`), no curator role, same password variable; check they import (realm reset if Aspire keeps the Keycloak volume, note how)
- [ ] T07.2 Under Aspire: as `dev-fr`, `/foodpedia?q=saumon` and `q=salmon` find *Saumon* with French UI; as `dev-vi`, `q=ca hoi` finds *cá hồi* with Vietnamese UI; as `dev`, English
- [ ] T07.3 A translated error: e.g. `q` over 100 characters typed in the URL (or another real problem) shows the message of its code in the user's language
- [ ] T07.4 Light and dark mode screenshots of the search page (paprika vs error red), saved in the session scratchpad, not committed
- [x] T07.5 Spike plan: Phase 4 **Status (date)** with the answers (Astryx in Server Components, theme build, dark mode; paprika vs error red; Vitest browser mode + MSW), what the tests fixed, what is not done (C5 moved to the curator app, E01.S06)
- [x] T07.6 Webapp `AGENTS.md`: Commands section true (both Vitest projects, Playwright Chromium install), drop the "set up during the rebuild" line for what now exists; adjust `webapp-test` / `webapp-ui` skills only where the T02–T05 reports found a convention that differs

## Done when

`git diff --stat` touches only docs, skills and the dev users file, and the observations of T07.2–T07.3 are written in the spike plan status.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`docs(repo): phase 4 findings (E01.S04.T07)`), stop.

Status: partial (T07.2-T07.4 blocked: Docker daemon not running)

Report:
- Changed: `dev-fr` / `dev-vi` added to the dev users file (valid JSON, no curator role; Keycloak has no data volume so the import runs on every start); spike plan Phase 4 status with the answers; webapp `AGENTS.md` Commands and rebuild note; `webapp-test` skill browser-project notes.
- Blocked (docker: `/var/run/docker.sock` missing, no daemon): `aspire run` built and started the dashboard but Postgres and Keycloak never started, API and webapp stayed Waiting. T07.2-T07.4 not observed, no screenshots; redo steps are in the spike plan status. Aspire stopped.
- Surprising: nothing else; `dev-user-password` is an Aspire generated secret, read it in the dashboard.
