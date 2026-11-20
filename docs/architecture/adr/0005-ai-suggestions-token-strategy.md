# ADR 0005 — AI suggestions: code computes, AI ranks

- **Status**: Accepted
- **Date**: 2026-11-20

## Context

Recipe suggestions are the core domain. They depend on stock, recipes, prices at preferred vendors, budget, and context signals (season, weather, mood, upcoming events). An AI agent produces the suggestions, but sending raw data to a model is slow, costly and not reproducible.

## Decision

**Code computes the facts; the AI only ranks and explains.**

1. **Deterministic pre-filtering** in the domain: candidate recipes by stock coverage, expiring items used, cost of missing items at preferred vendors, budget remaining for the period.
2. **Context signals are summarized by code**:
   - season — derived from the date (region later);
   - weather — fetched from an API, summarized to one line ("rainy, 9°C");
   - mood and upcoming events — provided by the user as free text (calendar later), interpreted by the AI.
3. **Compact payload**: only the top 10–20 candidates, one line each (`id | name | coverage | missing + cost | uses expiring`), plus the summarized signals.
4. **Structured output**: ranked recipe ids with a one-sentence explanation each.
5. **Small, fast model by default**; larger model only when needed.
6. **Caching** keyed by a hash of pantry state, budget, period and signals.
7. **Tools for details**: the agent fetches recipe steps or price history on demand instead of receiving them upfront.

## Consequences

- Token usage stays low and predictable; results are explainable and largely reproducible.
- Upstream contexts (Pantry, Market, Cookbook) must expose the data needed for pre-filtering (coverage, normalized quantities, prices per base unit).
- The LLM sits behind an anti-corruption layer; the provider can change without affecting the domain.
