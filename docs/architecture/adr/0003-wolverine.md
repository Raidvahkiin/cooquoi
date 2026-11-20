# ADR 0003 — Wolverine for messaging and handlers

- **Status**: Proposed — to be confirmed by a spike on the first context
- **Date**: 2026-11-20

## Context

Use cases are implemented as commands and queries. Bounded contexts must react to each other (`MealCooked` consumes stock, `ReceiptScanned` feeds Pantry and Market) and some features are time-based (expiry reminders). MediatR became commercial in 2025.

Options considered:

| Option | Summary |
|---|---|
| MediatR | Familiar, in-process only; commercial license (free community tier) |
| Mediator (martinothamar) | MIT, source-generated, MediatR-like API, compile-time checks; in-process only |
| **Wolverine** | MIT; mediator + messaging: local queues, transactional outbox/inbox on Postgres + EF Core, retries, scheduled messages, optional transports |
| No mediator | Endpoints call handlers directly; cross-cutting concerns via filters |

## Decision

Use **Wolverine**, pending a spike: Foodpedia or Pantry context with EF Core + Postgres, one command and one domain event through the outbox, running under Aspire.

## Consequences

- Durable domain/integration events, retries and scheduling come built in — no hand-made outbox.
- Lowest handler boilerplate (convention-based handlers, cascading messages).
- Learning curve and conventions ("magic"): use Wolverine's diagnostics and pre-generated code to keep handler discovery visible.
- Handlers must stay thin: business rules live in aggregates and value objects.
- Fallback if the spike fails: **Mediator** (source-generated) plus a separate outbox solution.
