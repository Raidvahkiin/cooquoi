# Glossary — Ubiquitous Language

Terms used identically in code, docs and conversations. When a new concept appears, define it here first.

## Identity & Household

| Term | Definition |
|---|---|
| **User** | A person authenticated through Keycloak. Has a **user profile** in Cooquoi, identified by our own `UserId`; no credentials are stored by Cooquoi. |
| **User profile** | What Cooquoi stores about a user: display name and email (copied from Keycloak at sign-in), language preference, default household. |
| **Household** | The owner of pantries, budget, preferred vendors and (later) private ingredients. In MVP every user gets a personal household on first login. |
| **Membership** | The link between a user and a household, with a role in that household. |
| **Curator** | A user with the curator role, allowed to add and edit Foodpedia entries. |

## Foodpedia

| Term | Definition |
|---|---|
| **Foodpedia** | The shared reference knowledge about food: ingredients, units, conversions, shelf life. Read by every other context. |
| **Ingredient** | A generic food concept ("salmon", "egg", "flour"), independent of brand or packaging. Seeded and maintained by curators. Identified by a language-neutral **code**; its names are translations. |
| **Localized name** | The primary name of an ingredient in one locale (`en`, `fr`, `vi`), plus optional **synonyms** in that locale. Missing translations fall back to English. |
| **Base unit** | The single unit in which an ingredient is normalized: **gram** (solids), **millilitre** (liquids) or **piece** (countables). |
| **Unit** | A unit a quantity can be expressed in: g, kg, ml, l, piece, and ingredient-specific ones like pack, fillet, slice. |
| **Dimension** | The kind of measure a unit belongs to: mass, volume or count. |
| **Conversion** | A rule turning one unit into another. **Universal** within a dimension (1 kg = 1000 g); **ingredient-specific** across dimensions or for custom units (1 egg ≈ 60 g, 1 fillet of salmon ≈ 200 g). |
| **Shelf life** | The default number of days an ingredient keeps, with a **recommended storage** (fridge, freezer, room). Used to propose an expiry date and a pantry. |
| **Tag** | A namespaced label classifying ingredients: `category:` (UI grouping, at least one per ingredient), `season:`, `diet:`, `allergen:`, `family:` (links variants such as salmon and smoked salmon). Curated, with translated labels. |
| **Variant** | A distinct ingredient close to another (smoked salmon / salmon, brown rice / white rice), linked by a `family:` tag. |
| **Household-private ingredient** *(after MVP)* | An ingredient created by a household when it is missing from Foodpedia; visible only to that household until a curator promotes it. |

## Pantry

| Term | Definition |
|---|---|
| **Pantry** | A named storage owned by a household ("Fridge", "Freezer", "Mum's cellar"). A household can have several. |
| **Stock item** | An ingredient held in a pantry, with a quantity and an optional expiry date. |
| **Entered quantity** | The quantity exactly as the user typed it (amount + unit, e.g. "2 packs"). Never overwritten. |
| **Normalized quantity** | The entered quantity converted to the ingredient's base unit, when a conversion is known. |
| **Unnormalized** | A stock item whose entered unit has no known conversion yet. It is visible but not summed with others. |
| **Household conversion** | A conversion confirmed by a household for its own products ("1 fillet ≈ 180 g"). Applied before Foodpedia's default conversions. |
| **Conversion proposal** | A suggested conversion ("1 fillet ≈ 200 g?") offered to the user when an unknown unit is entered; the user confirms or adjusts it. |
| **Expiry date** | The date a stock item should be consumed by. Defaults from shelf life, adjustable manually. |
| **Threshold** | Optional per-ingredient levels set by the household: **low** and **over**, expressed in the base unit. |
| **Low stock / Overstock** | The state of an ingredient whose total normalized quantity is below the low threshold / above the over threshold. |
| **Consume** | Remove a quantity of an ingredient from stock (cooking, eating). |
| **Discard** | Remove a stock item that was not consumed (expired, spoiled). |

## Market

| Term | Definition |
|---|---|
| **Vendor** | A shop or market where food is bought. No location in MVP. |
| **Preferred vendor** | A vendor a household usually buys from; suggestions only use prices from preferred vendors. |
| **Product** | A specific purchasable item ("Super Salmon 200 g"), linked to an ingredient and a pack size. |
| **Price observation** | The price of a product at a vendor at a given date. |

## Cookbook & Planning

| Term | Definition |
|---|---|
| **Recipe** | A dish described by ingredients with quantities, steps and a number of servings. |
| **Meal plan** | Recipes scheduled on days of a period for a household. |
| **Shopping list** | Missing quantities computed from a meal plan minus the current stock. |
| **Budget** | An amount, currency and period (day, week or month) a household wants to spend on food. |
| **Suggestion** | A recipe proposed and ranked by the AI agent, with a short explanation. |
| **Context signals** | Non-food inputs to suggestions: season, weather, mood, upcoming events. |
| **Coverage** | The share of a recipe's ingredients already available in stock. |

## Domain events (first pass)

`HouseholdCreated` · `IngredientAdded` · `ConversionDefined` · `ItemsRestocked` · `StockItemConsumed` · `StockItemDiscarded` · `StockItemExpiringSoon` · `StockRunningLow` · `StockOverstocked` · `PricePaidRecorded` · `RecipeRegistered` · `MealPlanned` · `MealCooked` · `ShoppingListGenerated` · `ReceiptScanned`
