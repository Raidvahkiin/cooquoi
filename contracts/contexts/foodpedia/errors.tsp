namespace Cooquoi.Foodpedia;

/** Stable error codes of the Foodpedia context (`problem.code`), next to `CommonErrorCode`. */
union FoodpediaErrorCode {
  codeTaken: "foodpedia.code-taken",
  invalidCode: "foodpedia.invalid-code",
  invalidName: "foodpedia.invalid-name",
  englishNameRequired: "foodpedia.english-name-required",
  duplicateLocale: "foodpedia.duplicate-locale",
  nameMissing: "foodpedia.name-missing",
  nameTaken: "foodpedia.name-taken",
  invalidBaseUnit: "foodpedia.invalid-base-unit",
  invalidConversionAmount: "foodpedia.invalid-conversion-amount",
  conversionNotNeeded: "foodpedia.conversion-not-needed",
  invalidShelfLife: "foodpedia.invalid-shelf-life",
  categoryTagRequired: "foodpedia.category-tag-required",
  ingredientRetired: "foodpedia.ingredient-retired",
  invalidTagCode: "foodpedia.invalid-tag-code",
  invalidTagLabel: "foodpedia.invalid-tag-label",
  englishLabelRequired: "foodpedia.english-label-required",
  unknownTags: "foodpedia.unknown-tags",
}
