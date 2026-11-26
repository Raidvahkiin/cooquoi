import "@typespec/http";
import "@typespec/openapi";
import "./models.tsp";

using Http;
using OpenAPI;
using Cooquoi.Common;

namespace Cooquoi.Foodventory;

@tag("Foodventory")
interface PantryOperations {
  /** Q1: the household's pantries with their item counts, and the count of items to put away. Not paged. */
  @route("/v1/pantries")
  @operationId("listPantries")
  listPantries is CollectionReadOperation<PantryList>;

  /**
   * C1: creates a Pantry aggregate.
   * Errors: `foodventory.name-taken` (409), `foodventory.invalid-name` (400).
   */
  @route("/v1/pantries")
  @operationId("createPantry")
  createPantry is CreateOperation<CreatePantry, CreatedPantry>;

  /**
   * C2: renames a Pantry aggregate.
   * Errors: `foodventory.name-taken` (409), `foodventory.invalid-name` (400).
   */
  @route("/v1/pantries/{pantryId}:rename")
  @operationId("renamePantry")
  renamePantry is ReplaceActionOperation<RenamePantry, PantryPath>;

  /** C3: sets or clears the storage of a Pantry aggregate. */
  @route("/v1/pantries/{pantryId}:set-storage")
  @operationId("setPantryStorage")
  setPantryStorage is ReplaceActionOperation<SetPantryStorage, PantryPath>;

  /**
   * C4: deletes an empty Pantry aggregate.
   * Errors: `foodventory.not-empty` (422).
   */
  @route("/v1/pantries/{pantryId}")
  @operationId("deletePantry")
  deletePantry is DeleteOperation<PantryPath>;
}
