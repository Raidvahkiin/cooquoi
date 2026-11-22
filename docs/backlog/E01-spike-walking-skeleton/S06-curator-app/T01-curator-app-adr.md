# E01.S06.T01 — ADR 0017: curator app

## Goal

An ADR (status `Proposed`) decides how the curator app, separate from the user app `@cooquoi/webapp`, is built, signed into, shared with the user app and run, so its tasks can be planned.

## Read only

- `docs/README.md` ADR table (lines 28-46) for the next number and the row format
- [ADR 0011](../../../architecture/adr/0011-webapp-api-auth.md) **Decision** (BFF, bearer-only API); [ADR 0013](../../../architecture/adr/0013-webapp-structure.md) **Decision** (webapp structure); [ADR 0004](../../../architecture/adr/0004-keycloak-identity.md) **Decision**; [ADR 0006](../../../architecture/adr/0006-cicd-github-actions-coolify.md) **Decision** (deployment)
- [ADR 0016](../../../architecture/adr/0016-package-manager-and-runtime.md) whole file, as the format to copy (headings only if long)
- `ts/apps/cooquoi-webapp/AGENTS.md` "Structure"
- `aspire/keycloak/realms/cooquoi-realm.json`: clients and the `catalog-curator` role only (`grep -n '"clientId"\|catalog-curator'`)

## Reference

- ADR format: `docs/architecture/adr/0016-package-manager-and-runtime.md`
- Today: one Next.js app (`ts/apps/cooquoi-webapp`) with `src/shared/{api,auth,config,i18n,ui}`; one Keycloak realm with the `catalog-curator` realm role; the API already enforces the role on curator endpoints

## Skills

none (docs only)

## Model

opus (new structure, no pattern to copy)

## Decisions to make (each with options and a recommendation; mark the ones the user must confirm)

1. App name and path (`@cooquoi/curator`, `ts/apps/cooquoi-curator`?) and whether it is a PWA (the user app is, ADR 0007)
2. Sign-in: its own Keycloak client or the webapp's; how the role is checked (token `realm_access.roles` in the BFF session, a guard in `(app)/layout.tsx` or `proxy.ts`), what a non-curator sees
3. Shared code: which of `shared/{api,auth,config,i18n,ui}`, the `cooquoi` theme, the error messages and the test helpers move to `ts/libs` (one lib or several, names), and how `messages/*.json` are split between shared (errors, units) and per-app
4. Aspire resource and port; deployment as a second Coolify service
5. What changes in `AGENTS.md` / skills (webapp skills apply to both apps?)

## Out of scope

- Any code; the follow-up tasks of S06 (planned after this ADR)

## Subtasks

- [ ] T01.1 `docs/architecture/adr/0017-curator-app.md`: Context, Decision, Alternatives, Consequences, status `Proposed`
- [ ] T01.2 Row in the `docs/README.md` ADR table
- [ ] T01.3 List in the report the decisions the user must confirm before the ADR is `Accepted`

## Done when

`git diff --stat` shows only the ADR and `docs/README.md`, and the ADR answers the five decisions above.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`docs(architecture): ADR 0017 curator app (E01.S06.T01)`), stop.

Status: todo

Report:
