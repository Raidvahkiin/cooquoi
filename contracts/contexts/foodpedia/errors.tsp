namespace Cooquoi.Foodpedia;

/** Stable error codes of the Foodpedia context (`problem.code`), next to `CommonErrorCode`. */
union FoodpediaErrorCode {
  codeTaken: "foodpedia.codeTaken",
  invalidCode: "foodpedia.invalidCode",
  invalidName: "foodpedia.invalidName",
  englishNameRequired: "foodpedia.englishNameRequired",
  duplicateLocale: "foodpedia.duplicateLocale",
  nameMissing: "foodpedia.nameMissing",
  nameTaken: "foodpedia.nameTaken",
  invalidBaseUnit: "foodpedia.invalidBaseUnit",
  invalidConversionAmount: "foodpedia.invalidConversionAmount",
  conversionNotNeeded: "foodpedia.conversionNotNeeded",
  invalidShelfLife: "foodpedia.invalidShelfLife",
  categoryTagRequired: "foodpedia.categoryTagRequired",
  ingredientRetired: "foodpedia.ingredientRetired",
  invalidTagCode: "foodpedia.invalidTagCode",
  invalidTagLabel: "foodpedia.invalidTagLabel",
  englishLabelRequired: "foodpedia.englishLabelRequired",
  unknownTags: "foodpedia.unknownTags",
}
