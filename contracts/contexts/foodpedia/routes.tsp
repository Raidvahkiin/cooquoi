import "@typespec/http";
import "@typespec/openapi";
import "./models.tsp";

using Http;
using OpenAPI;
using Cooquoi.Common;

namespace Cooquoi.Foodpedia;

@tag("Foodpedia")
interface IngredientOperations {
  /**
   * Q1: searches primary names and synonyms in all locales, accent-insensitive, active locale ranked first.
   * Names come in the `Accept-Language` locale (English fallback). `q` is at most 100 characters.
   * Sort keys: `relevance`, `name`, `code`; default `-relevance,name` with `q`, `name` without.
   */
  @route("/v1/ingredients")
  @operationId("searchIngredients")
  searchIngredients is ListOperation<IngredientFilter, IngredientSummary>;

  /** Q2: one ingredient, name and synonyms in the `Accept-Language` locale (English fallback). */
  @route("/v1/ingredients/{ingredientId}")
  @operationId("getIngredient")
  getIngredient is ReadOperation<IngredientDetail, IngredientPath>;

  /**
   * C1: creates an ingredient. Requires the curator role.
   * Errors: `foodpedia.codeTaken`, `foodpedia.nameTaken` (409), `foodpedia.unknownTags` (422).
   */
  @route("/v1/ingredients")
  @operationId("createIngredient")
  createIngredient is CreateOperation<CreateIngredient, CreatedIngredient>;

  /** C5: defines or replaces the conversion of one unit. Requires the curator role. */
  @route("/v1/ingredients/{ingredientId}:define-conversion")
  @operationId("defineConversion")
  defineConversion is ReplaceActionOperation<DefineConversion, IngredientPath>;

  /**
   * C11: sets how the expiry date of one ingredient is known. Requires the curator role.
   * Errors: `foodpedia.ingredientRetired` (422).
   */
  @route("/v1/ingredients/{ingredientId}:set-expiry-kind")
  @operationId("setExpiryKind")
  setExpiryKind is ReplaceActionOperation<SetExpiryKind, IngredientPath>;
}
