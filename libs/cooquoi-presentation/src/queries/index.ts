import { GetIngredientQueryHandler } from "./get-ingredient.query";
import { GetIngredientsGridQueryHandler } from "./get-ingredients-grid.query";
import { GetManyIngredientsQueryHandler } from "./get-many-ingredients.query";

export * from "./get-ingredient.query";
export * from "./get-ingredients-grid.query";
export * from "./get-many-ingredients.query";

export const queryHandlers = [
  GetIngredientQueryHandler,
  GetIngredientsGridQueryHandler,
  GetManyIngredientsQueryHandler,
];
