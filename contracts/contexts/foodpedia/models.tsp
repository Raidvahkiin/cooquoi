import "@typespec/http";
import "../../common/main.tsp";

using Http;
using Cooquoi.Common;

namespace Cooquoi.Foodpedia;

/** Lifecycle of an ingredient; retired ones are hidden from non-curators. */
union IngredientStatus {
  active: "active",
  retired: "retired",
}

/** Where an ingredient keeps best. */
union Storage {
  fridge: "fridge",
  freezer: "freezer",
  room: "room",
}

/** How the expiry date of an ingredient is known (F12). */
union ExpiryKind {
  printed: "printed",
  estimated: "estimated",
  longLife: "longLife",
}

/** Typed filters of the ingredient search (Q1). */
model IngredientFilter {
  /** Tag codes (`<namespace>:<value>`); all must match. */
  @query(#{ explode: true }) tags?: string[];

  /** Curators only; others always see active ingredients. */
  @query status?: IngredientStatus;
}

/** Path of one ingredient, named as the backend route names it. */
alias IngredientPath = {
  @path ingredientId: Id;
};

/** One search result; the name is in the requested locale, English when missing. */
model IngredientSummary {
  id: Id;
  code: string;
  name: string;
  baseUnit: BaseUnit;
  categoryTags: string[];
}

/** How many base units one `unit` of this ingredient weighs or holds, e.g. 1 `clove` = 5 `g`. */
model Conversion {
  unit: Unit;
  unitAmount: decimal;
  baseAmount: decimal;
}

/** How long an ingredient keeps, and where. */
model ShelfLife {
  days: int32;
  recommendedStorage: Storage;
}

/** One ingredient in the requested locale; `version` is sent back as `If-Match`. */
model IngredientDetail {
  id: Id;
  code: string;
  locale: Locale;
  name: string;
  synonyms: string[];
  baseUnit: BaseUnit;
  expiryKind: ExpiryKind;
  conversions: Conversion[];
  shelfLife: ShelfLife | null;
  tags: string[];
  status: IngredientStatus;
  version: int64;
}

/** A name of the ingredient in one locale. */
model IngredientName {
  locale: Locale;
  primary: string;
  synonyms?: string[];
}

/** Body of the ingredient creation (C1); business rules F1–F9. */
model CreateIngredient {
  /** Kebab-case, unique. */
  code: string;

  /** One per locale; English required. */
  names: IngredientName[];

  baseUnit: BaseUnit;

  expiryKind: ExpiryKind;

  /** At least one `category:` tag. */
  tags: string[];

  conversions?: Conversion[];
  shelfLife?: ShelfLife;
}

/** Id of the created ingredient. */
model CreatedIngredient {
  id: Id;
}

/** Body of an expiry kind change (C11). */
model SetExpiryKind {
  expiryKind: ExpiryKind;
}

/** Body of a conversion definition (C5); replaces an existing one for the same unit. */
model DefineConversion {
  unit: Unit;
  unitAmount: decimal;
  baseAmount: decimal;
}
