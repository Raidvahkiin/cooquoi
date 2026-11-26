namespace Cooquoi.Foodventory;

/** Stable error codes of the Foodventory context (`problem.code`), next to `CommonErrorCode`. */
union FoodventoryErrorCode {
  nameTaken: "foodventory.nameTaken",
  invalidName: "foodventory.invalidName",
  notEmpty: "foodventory.notEmpty",
}
