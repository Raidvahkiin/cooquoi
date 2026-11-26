# E02.S08 — Planning read model and verification under Aspire

## Goal

The Foodventory context publishes `GetStockSummary(householdId, asOf, expiringWithinDays = 3)` and `GetStock(householdId, ingredientIds)` in-process for Planning & Suggestion, and exposes the same `StockSummary` over HTTP. A script verifies the epic end to end under Aspire through the API only, signed in as dev@cooquoi.local, and the findings are recorded.

## Acceptance

- [ ] `PublicApi/StockSummaryQuery` (or similar) returns `StockSummary`:
  - `stateHash`: a hash of the household's IngredientStock versions; it changes on any stock write and is stable otherwise.
  - Per ingredient held or with a threshold: `ingredientId`, `code`, `baseUnit`, effective `expiryKind`, `total` (put away or not), `earliestExpiry`.
  - `expiringAmount`: within the window or expired, never `longLife`.
  - `unknownExpiryAmount`: missing expiry, never counted as expiring.
  - `unnormalized[]` (never summed) and `state`.
- [ ] `GetStock` returns the same entries for a subset of ingredient ids.
- [ ] `GET /v1/stock/summary?expiringWithinDays=3` returns the same `StockSummary` for the caller's household (open question Q11). Drift `none`.
- [ ] Verification: `aspire/http/foodventory.http` (or `scripts/verify-foodventory.sh`) as dev@cooquoi.local does these steps, all through the API, and the summary shows the expected totals, `expiringAmount` and `state`:
  1. `POST /v1/me:sign-in`; list the default Pantry aggregates.
  2. Restock milk (printed, no date), carrots (estimated) and rice (longLife).
  3. Read `pantryId=none`; put the milk away into the Fridge with a date; split the carrots between Fridge and Freezer.
  4. Consume more carrots than held (shortfall reported).
  5. Set a threshold on rice.
  6. Read the summary.
- [ ] `findings.md` records what worked, what surprised and what is open, and the epic table and README are updated.

## Context

- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) "Published API for Planning & Suggestion"
- [ADR 0005](../../../architecture/adr/0005-ai-suggestions-token-strategy.md) "Decision" §6 (cache key)
- `dotnet/src/Cooquoi.Context.Foodventory/Features/Shared/IngredientStockRows.cs` (from E02.S07.T03)
- `aspire/keycloak/realms/cooquoi-realm.json:31-51`, `aspire/keycloak/dev-users/cooquoi-users-0.json:5-19`, `aspire/Cooquoi.AppHost/Resources/Identity.cs:24-26`
- `docs/backlog/E01-spike-walking-skeleton/S07-redis-session-store/findings.md` (findings shape)
- Open questions Q1, Q3, Q11 ([epic](../epic.md#open-questions))

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-contract-summary.md) | Contract: stock summary | todo | E02.S07 | yes | |
| [T02](T02-stock-summary.md) | `GetStockSummary` / `GetStock` in-process and over HTTP | todo | T01 | no | |
| [T03](T03-verify-and-findings.md) | Verify under Aspire through the API, findings | todo | T02 | no | |

The story review runs after T03, followed by the epic close (`backlog-run`).

## Summary

(written when done)
