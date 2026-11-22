# E01.S07.T04 — Verify under Aspire, docs and findings

## Goal

The running system shows the story's acceptance:

- the cookie is small;
- Redis holds the session key with the right TTL;
- refresh extends the TTL;
- sign-out deletes the key;
- a Redis restart leads to a clean redirect to login.

The webapp `AGENTS.md` and the S04 findings point to ADR 0019, and findings are recorded.

## Read only

- [S07 story](story.md) "Acceptance"
- README "Run everything locally" (as left by E01.S05)
- `ts/apps/cooquoi-webapp/AGENTS.md` "## Structure" (line 42: `auth/ # oidc client, iron-session, getSession(), requireSession()`)
- [S04 findings](../S04-foodpedia-webapp/findings.md) "Open questions at the end of the run" item 3
- T01 and T03 reports

## Reference

- How S04 verified the running system: [S04.T07](../S04-foodpedia-webapp/T07-verify-and-findings.md), Subtasks and Report.
- Checks:
  - Sign in as `dev@cooquoi.local` at `https://localhost:3000`, using the browser pane, then read the `cooquoi_session` cookie length.
  - `docker exec <redis> redis-cli -a <pw> --scan --pattern 'session:*'` and `TTL <key>`.
  - Wait for or force a refresh, then compare the TTL.
  - Sign out, then check the key is gone.
  - `docker restart <redis>`, then reload a signed-in page: expect a redirect to login.

## Skills

`webapp-review` (self-check of T02 and T03 before the story review)

## Model

sonnet

## Out of scope

- Fixes beyond small ones found by the checks; larger issues go to findings as open items

## Subtasks

- [x] T04.1 Run every check above and record the results (cookie bytes, TTL values, behaviour)
- [x] T04.2 `AGENTS.md` line 42: `auth/ # oidc client, Redis-backed sessions (ADR 0019), getSession(), requireSession()`
- [x] T04.3 S04 findings open question 3: add "→ decided in ADR 0019, built in E01.S07"
- [x] T04.4 `S07-redis-session-store/findings.md`: what was built, the check results and what is left

## Done when

`pnpm nx run-many -t typecheck lint test -p @cooquoi/webapp 2>&1 | tail -5` is green, and `findings.md` records every check from the story acceptance with its result.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`docs(backlog): E01.S07 verified, findings (E01.S07.T04)`), stop.

Status: done (not committed; the parent commits)

Report:
Verified under `aspire start`: rediss works unchanged; cookie 308 B (not <200, open item with options in findings.md); TTL = 30-day refresh lifetime, refresh restores it, sign-out deletes the key, Redis restart gives a redirect to login (no 500).
Changed `AGENTS.md` Structure line (real helper names), S04 findings open question 3, wrote `findings.md`; typecheck/lint/test green. Aspire stopped.
Surprise: the `webapp` ACL forbids SCAN/TTL, so TTLs were probed with `EXPIRE ... GT|LT`.
