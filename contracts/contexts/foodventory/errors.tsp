namespace Cooquoi.Foodventory;

/** Stable error codes of the Foodventory context (`problem.code`), next to `CommonErrorCode`. */
union FoodventoryErrorCode {
  nameTaken: "foodventory.name-taken",
  invalidName: "foodventory.invalid-name",
  notEmpty: "foodventory.not-empty",
}
