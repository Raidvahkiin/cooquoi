# E02.S08.T03 — Verify under Aspire through the API, findings

## Goal

Under `aspire start`, a committed script signs in as dev@cooquoi.local and exercises the epic through the API only: restock, put away, consume, threshold, then the stock summary. The results match expectations, and `findings.md` records what was learned.

## Read only

- `dotnet/AGENTS.md` "Commands" (`aspire start`, ports)
- `aspire/keycloak/realms/cooquoi-realm.json:31-51` (clients, `directAccessGrantsEnabled: false`), `aspire/keycloak/dev-users/cooquoi-users-0.json:5-19`, `aspire/Cooquoi.AppHost/Resources/Identity.cs:24-26` (dev users and password parameter)
- [ADR 0004](../../../architecture/adr/0004-keycloak-identity.md) and [ADR 0011](../../../architecture/adr/0011-webapp-api-auth.md) "Decision" (does a dev-only password-grant client conflict?)
- `docs/backlog/E01-spike-walking-skeleton/S04-foodpedia-webapp/T07-verify-and-findings.md` Subtasks and Report (how E01 verified with curl)
- `docs/backlog/E01-spike-walking-skeleton/S07-redis-session-store/findings.md` (shape)
- Open questions Q1, Q2, Q3 ([epic](../epic.md#open-questions))

## Reference

- Token (Q1, decided 2026-11-22): a dev-only public client `cooquoi-dev-cli` with `directAccessGrantsEnabled: true`, audience `cooquoi-api`, imported only in development, like the dev users (`Identity.cs:26`). The script then runs `curl -d grant_type=password -d client_id=cooquoi-dev-cli -d username=dev@cooquoi.local --data-urlencode password=…` against the realm token endpoint. The password comes from the Aspire parameter, never committed.
- Script: `aspire/http/foodventory.http` (REST Client / JetBrains format, variables for host and token) or `scripts/verify-foodventory.sh` with `curl` + `jq` that asserts each step. Prefer the shell script, because it can assert and run in one command. Every POST sends a fresh `Idempotency-Key`; replace actions send the ETag read just before.
- Dev database (Q3, decided 2026-11-22: reset, no backfill): if dev@cooquoi.local signed in before E02.S02, the household has no default Pantry aggregates. Reset the Postgres data volume first (`docker volume ls | grep postgres`) and say so in the findings.
- Steps and expected results:
  1. `POST /v1/me:sign-in`, then `GET /v1/pantries` → Fridge, Freezer, Dry goods.
  2. Restock milk 1 L (printed, no date → `expiryMissing`), carrots 1 kg (estimated → proposed date), rice 2 kg (longLife).
  3. `GET /v1/stock-items?pantryId=none` → 3 items with suggested pantries.
  4. Move milk to the Fridge with a date 2 days out; move 400 g of carrots to the Freezer (split), the rest to the Fridge.
  5. Consume carrots 2 kg → shortfall 1 kg; the carrots leave.
  6. Set the rice threshold low 3000 g → `StockRunningLow`, state `low`.
  7. `GET /v1/stock/summary` → milk `expiringAmount` = 1000 ml (or the base unit); rice `low`, `unknownExpiryAmount` 0; `stateHash` present. Repeat → same hash.

## Skills

`run` (launch under Aspire), none for code

## Model

sonnet

## Out of scope

Any UI; Planning code; changing endpoints (fix bugs found here in a follow-up task, listed in the findings)

## Subtasks

- [ ] T03.1 Token path per Q1 (realm import change, if any, development only)
- [ ] T03.2 The script with assertions
- [ ] T03.3 `aspire start`; run the script; capture output (scratchpad, not committed)
- [ ] T03.4 `findings.md` (worked / surprised / open); story Summary; epic table; `docs/backlog/README.md` E02 status
- [ ] T03.5 Stop Aspire

## Done when

The script exits 0 under `aspire start` and prints each step's check. `dotnet build aspire/Cooquoi.AppHost -v q -nologo -clp:ErrorsOnly` is green if the realm import changed. `findings.md` is committed.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`docs(backlog): E02 verified through the API, findings (E02.S08.T03)`), stop.

Status: todo

Report:
