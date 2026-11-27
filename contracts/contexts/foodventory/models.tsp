import "@typespec/http";
import "../../common/main.tsp";

using Http;
using Cooquoi.Common;

namespace Cooquoi.Foodventory;

/** Where a Pantry aggregate keeps its stock. */
union StorageType {
  fridge: "fridge",
  freezer: "freezer",
  room: "room",
}

/** Path of one Pantry aggregate, named as the backend route names it. */
alias PantryPath = {
  @path pantryId: Id;
};

/** One Pantry aggregate in the list; `version` is sent back as `If-Match`. */
model PantrySummary {
  id: Id;
  name: string;
  storage?: StorageType;
  itemCount: int32;
  version: int64;
}

/** The household's pantries and how many stock items still wait to be put away (Q1). */
model PantryList {
  pantries: PantrySummary[];
  toPutAwayCount: int32;
}

/** Body of a Pantry aggregate creation (C1). */
model CreatePantry {
  name: string;
  storage?: StorageType;
}

/** Id of the created Pantry aggregate. */
model CreatedPantry {
  id: Id;
}

/** Body of a Pantry aggregate rename (C2). */
model RenamePantry {
  name: string;
}

/** Body of a storage change (C3); `null` clears it. */
model SetPantryStorage {
  storage: StorageType | null;
}

/** How the expiry date of an ingredient is known; same values as Foodpedia's. */
union ExpiryKind {
  printed: "printed",
  estimated: "estimated",
  longLife: "longLife",
}

/** Where an expiry date comes from. */
union ExpirySource {
  proposed: "proposed",
  manual: "manual",
}

/** An amount in a unit. */
model Quantity {
  amount: decimal;
  unit: Unit;
}

/** An expiry date and where it comes from. */
model Expiry {
  date: plainDate;
  source: ExpirySource;
}

/** Path of one stock item, named as the backend route names it. */
alias StockItemPath = {
  @path stockItemId: Id;
};

/** One stock item; `stockVersion` is the IngredientStock aggregate ETag for later `If-Match` writes. */
model StockItemView {
  id: Id;
  ingredientId: Id;
  ingredientName: string;
  pantryId?: Id;
  entered: Quantity;
  remaining: Quantity;

  /** In the base unit; absent when the item is unnormalized. */
  normalized?: Quantity;

  expiry?: Expiry;

  /** Effective kind. */
  expiryKind: ExpiryKind;

  expiryMissing: boolean;

  /** The date is far outside the household's usual range (Q8). */
  expiryUnusual: boolean;

  ingredientRetired: boolean;
  addedOn: plainDate;
  suggestedPantryId?: Id;
  stockVersion: int32;
}

/** One line of a restock. */
model RestockLine {
  ingredientId: Id;
  amount: decimal;
  unit: Unit;
  expiry?: plainDate;
}

/** Body of a restock (S1). */
model Restock {
  @minItems(1) lines: RestockLine[];
}

/** Offer to define a household conversion for an unnormalized line; no amounts when Foodpedia has no default. */
model ConversionProposal {
  ingredientId: Id;
  unit: Unit;
  unitAmount?: decimal;
  baseAmount?: decimal;
  baseUnit: BaseUnit;
}

/** The created stock items and the conversions worth defining. */
model RestockResult {
  items: StockItemView[];
  conversionProposals: ConversionProposal[];
}

/** Body of a stock item move (S2); `amount` and `unit` together move part of it. */
model MoveStockItem {
  pantryId: Id;
  amount?: decimal;
  unit?: Unit;
  expiry?: plainDate;
}

/** The moved item, and the source item when a split left a remainder. */
model MoveResult {
  moved: StockItemView;
  remainder?: StockItemView;
}
