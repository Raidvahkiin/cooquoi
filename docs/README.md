# Cooquoi documentation

Business and architecture documentation. Language-agnostic: it describes **what** and **why**.
Implementation details (**how** in a given codebase) live next to the code, in each project's `AGENTS.md`.

## Business

- [Vision](business/vision.md) — what Cooquoi is, actors, key journeys, scope
- [Glossary](business/glossary.md) — ubiquitous language shared by code, docs and conversations
- [Context map](business/context-map.md) — bounded contexts and how they relate

## Bounded contexts

Deep dives per context will live in `contexts/<name>/` (domain model, invariants, use cases).

| Context | Status |
|---|---|
| Foodpedia | Next |
| Pantry | Next |
| Market | Exists in legacy NestJS — to be revised |
| Cookbook | Later |
| Planning & Suggestion | Later |
| Receipt Scanning | Later |
| Identity & Household | Foundation (minimal) |

## Architecture decisions

| ADR | Title | Status |
|---|---|---|
| [0001](architecture/adr/0001-toolchain-folder-layout.md) | Toolchain-based folder layout | Accepted |
| [0002](architecture/adr/0002-csharp-backend-with-aspire.md) | C# backend orchestrated with Aspire | Accepted |
| [0003](architecture/adr/0003-wolverine.md) | Wolverine for messaging and handlers | Proposed |
| [0004](architecture/adr/0004-keycloak-identity.md) | Keycloak as identity broker | Accepted |
| [0005](architecture/adr/0005-ai-suggestions-token-strategy.md) | AI suggestions: code computes, AI ranks | Accepted |
| [0006](architecture/adr/0006-cicd-github-actions-coolify.md) | CI/CD: GitHub Actions + Coolify on a VPS | Accepted |
| [0007](architecture/adr/0007-mobile-strategy.md) | Mobile: PWA first, Flutter as the future native target | Accepted |
| [0008](architecture/adr/0008-web-ui-astryx.md) | Web UI: Astryx design system | Accepted |
| [0009](architecture/adr/0009-api-contract-typespec.md) | API contract: TypeSpec contract-first, typed list queries | Accepted |
| [0010](architecture/adr/0010-modular-monolith-persistence.md) | Modular monolith and persistence | Accepted |
| [0011](architecture/adr/0011-webapp-api-auth.md) | Webapp ↔ API: Next.js as backend-for-frontend, bearer-only API | Accepted |
| [0012](architecture/adr/0012-internationalization.md) | Internationalization: English, French, Vietnamese | Accepted |
| [0013](architecture/adr/0013-webapp-structure.md) | Webapp structure and conventions | Accepted |

## Conventions

- Diagrams are written in **Mermaid** inside Markdown.
- One ADR per decision, numbered, never rewritten once accepted — supersede it with a new ADR instead.
- Use the terms defined in the [glossary](business/glossary.md); add new terms there first.
