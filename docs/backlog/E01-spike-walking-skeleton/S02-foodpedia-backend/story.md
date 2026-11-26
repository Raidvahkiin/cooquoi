# E01.S02 — Foodpedia backend (phase 2)

## Goal

Foodpedia context with Q1, Q2, C1, C5 over HTTP, the agreed error contract, outbox events, idempotency and concurrency, and the decision on ADR 0003 (Wolverine).

## Acceptance

- [x] Q1 search, Q2 get, C1 create (curator), C5 define-conversion (`If-Match`) work through HTTP
- [x] Rules F1–F10 covered by domain tests; integration and architecture tests green
- [x] Vietnamese accent-insensitive search ("ca hoi" → "Cá hồi", `đ`)
- [x] ADR 0003 decision: Wolverine holds

## Context

- [Spike plan, Phase 2](../../../architecture/spike-walking-skeleton.md) ("Status (2026-11-21)" and "Answers")

## Tasks

Built before the backlog existed; no task files.

## Summary

Done 2026-11-21 (112 .NET tests green, verified under Aspire). Contract changes handed to S03: new codes `common.invalidSort`, `common.idempotencyKeyRequired`, `common.requestAlreadyProcessed`, `foodpedia.*`; actions `204` + `ETag`; create `201` + `Location` + `{ id }`; reads send `ETag`.
