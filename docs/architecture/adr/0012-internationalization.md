# ADR 0012 — Internationalization: English, French, Vietnamese

- **Status**: Accepted
- **Date**: 2026-11-21

## Context

Cooquoi must support **English (`en`), French (`fr`) and Vietnamese (`vi`)**. Retrofitting languages later is costly in two places:

- **Data**: Foodpedia is seeded with ingredient names; if names are a single column used for identity, uniqueness and search, adding languages means migrating ids, constraints, search indexes and seed files.
- **UI**: hard-coded strings spread across components must all be found and extracted.

Both are cheap to design for from the first context.

Three kinds of text exist, each handled differently:

| Kind | Examples | Translated? |
|---|---|---|
| **UI text** | Buttons, labels, errors, emails | Yes — message catalogs |
| **Reference data** | Foodpedia ingredient names, synonyms, categories, custom unit labels | Yes — stored translations |
| **User content** | Pantry names, recipe titles and steps, notes | **No** — stored as typed |

## Decision

### 1. Locales

- Supported: `en`, `fr`, `vi`. **Default and fallback: `en`.**
- Codes are BCP 47 language tags; region variants (e.g. `fr-CA`) fall back to the language.
- Locale resolution: **user preference** (stored in the user's profile / household membership) → browser `Accept-Language` → `en`.
- Authenticated app routes carry **no locale in the URL** (preference-based). Public pages (landing) may use a locale prefix later for SEO.

### 2. UI text (webapp)

- **next-intl** for Next.js App Router (Server and Client Components).
- One message catalog per locale under the webapp (`messages/en.json`, `fr.json`, `vi.json`), **ICU message format** (plurals, gender, placeholders) — also the format Flutter's ARB files use, so keys and messages can be shared later.
- **No hard-coded user-facing strings** in components from the first screen, even while only one language is complete.
- Formatting through `Intl` (dates, numbers, units, currency) using the active locale — never hand-formatted strings.
- Keycloak login and account pages: enable `en`, `fr`, `vi` in the realm and pass the locale (`ui_locales`) during sign-in.

### 3. Reference data (Foodpedia)

- **Identity is language-neutral.** An ingredient has an id and a stable `code` (e.g. `salmon`, `egg`), never a name as key.
- **Names are translations**: each ingredient has one **primary name per locale** plus optional **synonyms per locale** (`fr`: "courgette"; `en`: "zucchini", synonym "courgette").
- Uniqueness is per **(locale, name)**, not global.
- A missing translation falls back to `en`; curators see which translations are missing.
- Unit codes (`g`, `ml`, `piece`) are neutral; their labels are UI text. Ingredient-specific units ("fillet", "clove") carry translations like names.
- **Seed data is written in all three languages from the start.**

### 4. Search

- Search matches names and synonyms **in all locales**, ranking the active locale first — a French UI user typing "salmon" still finds *saumon*.
- Postgres **`unaccent` + `pg_trgm`** ([ADR 0010](0010-modular-monolith-persistence.md)) so accent-free typing works, which matters especially for Vietnamese ("ca hoi" → *cá hồi*). The spike must verify `unaccent` covers Vietnamese characters, including `đ`.

### 5. API

- Clients send **`Accept-Language`**; read endpoints return reference data **in the requested locale** (with fallback), e.g. `name: "Saumon"`.
- Curator endpoints read and write **all translations** (`LocalizedText` model in the contract: `{ locale, name, synonyms[] }[]`).
- Errors: `ProblemDetails` carries a **stable machine `code`** (e.g. `foodventory.conversion-unknown`) plus an English `detail`; clients localize by `code` ([ADR 0009](0009-api-contract-typespec.md)).
- The server never localizes user content.

### 6. Money and formatting

- Currency belongs to the household budget (`Money` = amount + ISO 4217 currency), independent of the UI language. EUR by default; VND must be supported (no minor unit), so amounts use the currency's own precision.
- Dates are stored in UTC / as plain dates; display is localized by the client.

### 7. AI

- Suggestions and explanations are generated **in the user's locale**; the locale is part of the prompt and of the suggestion cache key ([ADR 0005](0005-ai-suggestions-token-strategy.md)).
- Prompts sent to the model use **ingredient codes and English names** for compactness; only the output is localized.

### 8. Fonts

- The chosen fonts must support **Vietnamese diacritics**. Load them with `next/font` including the `vietnamese` subset; if a font of the Astryx theme (default Figtree) lacks Vietnamese support, replace it in the Cooquoi theme with one that has it (e.g. Be Vietnam Pro, Inter).

## Consequences

- Adding a fourth language later is a translation task (catalog + seed translations), not a migration.
- Every screen and every Foodpedia entry costs a bit more upfront (keys instead of literals, three seed names).
- Search works across languages and without accents.
- Translation completeness becomes something to track (missing keys in catalogs, missing ingredient translations).
