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
   * Errors: `foodventory.nameTaken` (409), `foodventory.invalidName` (422).
   */
  @route("/v1/pantries")
  @operationId("createPantry")
  createPantry is CreateOperation<CreatePantry, CreatedPantry>;

  /**
   * C2: renames a Pantry aggregate.
   * Errors: `foodventory.nameTaken` (409), `foodventory.invalidName` (422).
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
   * Errors: `foodventory.notEmpty` (422).
   */
  @route("/v1/pantries/{pantryId}")
  @operationId("deletePantry")
  deletePantry is DeleteOperation<PantryPath>;
}

@tag("Foodventory")
interface StockOperations {
  /**
   * S1: restocks the household with one or more ingredient lines.
   * Errors: `foodventory.ingredientRetired` (422), `foodventory.invalidQuantity` (422).
   */
  @route("/v1/stock:restock")
  @operationId("restock")
  restock is ActionWithResultOperation<Restock, RestockResult>;

  /**
   * S2: puts a stock item away in a Pantry aggregate, whole or in part.
   * Errors: `foodventory.quantityExceedsRemaining` (422), `foodventory.invalidQuantity` (422),
   * `foodventory.unitNotConvertible` (422; `params.ingredientId`, `params.unit`).
   */
  @route("/v1/stock-items/{stockItemId}:move")
  @operationId("moveStockItem")
  moveStockItem is ActionWithResultOperation<MoveStockItem, MoveResult, StockItemPath>;

  /**
   * S3: consumes an ingredient across its stock items; the response lists what was touched and any shortfall.
   * Errors: `foodventory.invalidQuantity` (422), `foodventory.unitNotConvertible` (422; `params.ingredientId`, `params.unit`).
   */
  @route("/v1/stock/{ingredientId}:consume")
  @operationId("consumeIngredient")
  consumeIngredient is ActionWithResultOperation<ConsumeIngredient, ConsumeResult, IngredientStockPath>;

  /**
   * S4: consumes part of one stock item; `unit` defaults to the entered unit.
   * Errors: `foodventory.invalidQuantity` (422), `foodventory.unitNotConvertible` (422; `params.ingredientId`, `params.unit`).
   */
  @route("/v1/stock-items/{stockItemId}:consume")
  @operationId("consumeStockItem")
  consumeStockItem is ActionWithResultOperation<ConsumeStockItem, ConsumeResult, StockItemPath>;

  /** S5: discards a stock item, with the reason. */
  @route("/v1/stock-items/{stockItemId}:discard")
  @operationId("discardStockItem")
  discardStockItem is ActionOperation<DiscardStockItem, StockItemPath>;

  /**
   * S6: corrects the remaining amount after a stock check; requires `If-Match` (the IngredientStock ETag).
   * Errors: `foodventory.invalidQuantity` (422).
   */
  @route("/v1/stock-items/{stockItemId}:adjust")
  @operationId("adjustStockItem")
  adjustStockItem is ReplaceActionOperation<AdjustStockItem, StockItemPath>;

  /** S7: sets or clears the expiry date; the source becomes `manual`. Requires `If-Match`. */
  @route("/v1/stock-items/{stockItemId}:set-expiry")
  @operationId("setStockItemExpiry")
  setStockItemExpiry is ReplaceActionOperation<SetStockItemExpiry, StockItemPath>;
}
