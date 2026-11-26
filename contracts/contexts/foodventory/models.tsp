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
