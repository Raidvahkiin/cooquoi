# Vision

> Cooquoi helps me decide **what to cook and what to buy**, from what is already in my fridge, my preferred markets and my budget — with as little typing as possible.

The scope is **daily comfort**: a small web app (mobile later) to manage the fridge and meals without effort.

## Capabilities

- Check what is in stock, and what is about to expire
- Check prices when something needs to be bought
- Register recipes and get recipe suggestions
- Plan meals for the week
- Share storage and recipes with family or friends *(after MVP)*

## The "wow"

1. **Recipe suggestions** based on what I already have, the prices at my preferred markets and my budget — enriched with context such as season, weather, mood and upcoming events.
2. **Receipt recognition** *(later)*: an AI reads a receipt photo and adds the items to stock — and records the prices paid at the same time.

The suggestion capability is the **core domain**. Every other context exists to feed it.

## Actors

| Actor | Description | When |
|---|---|---|
| **User** | A person managing their food. In MVP, a single individual | MVP |
| **Household** | The owner of pantries, budget and preferences. In MVP, one person; later, a family or group of friends | MVP (single member) |
| **Curator** | A user with an elevated role who maintains Foodpedia (ingredients, units, conversions) | MVP |
| **AI agent** | System actor that ranks and explains suggestions, and later reads receipts | Suggestions: after Pantry |
| **External price API** | Automatic price updates | Later |

## Key journeys

| # | Journey | Flow |
|---|---|---|
| J0 | **Get started** | I sign in (Google or a Cooquoi account) → my personal household and profile are created → I pick my language (`en` / `fr` / `vi`) → I land on an empty fridge that invites me to add my first items |
| J1 | **Restock** | I buy groceries → I add items (manually in MVP, receipt scan later) → stock increases, prices paid are recorded |
| J2 | **Check my fridge** | I see what I have, how much, and what expires soon |
| J3 | **Cook** | I cook a recipe → its ingredients are consumed from stock |
| J4 | **Register a recipe** | I save a recipe: ingredients with quantities, steps, servings |
| J5 | **Get suggestions** | "What can I cook tonight?" / "Plan my week under €40" → recipes ranked by stock coverage, expiring items, cost of missing items and context |
| J6 | **Plan & shop** | I plan the week → a shopping list of missing items is generated → I see where it is cheapest among my preferred vendors |

## Data sources

- **MVP**: user input, plus a seeded Foodpedia of common ingredients.
- **Later**: receipt scanning (AI), external price APIs, weather API, optional calendar.

## Scope

**In MVP**

- Authentication (Keycloak) and a personal household created on first login
- Foodpedia: seeded ingredients, base units, conversions, default shelf life, curator role
- Pantry: named pantries, stock items with entered and normalized quantities, expiry, thresholds

**After MVP**

- Market revision (vendors, observed prices, preferred vendors), Cookbook, Planning & Suggestion
- Household-private ingredients promotable to Foodpedia
- Sharing pantries and recipes, multi-member households
- Receipt scanning, external price updates
- Pantry physical locations, vendor locations

**Out of scope**

- Payments (not yet)
- Delivery
