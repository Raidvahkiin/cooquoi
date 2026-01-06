import { GetIngredientQueryHandler } from "./get-ingredient.query";
import { GetIngredientsGridQueryHandler } from "./get-ingredients-grid.query";

export * from "./get-ingredient.query";
export * from "./get-ingredients-grid.query";

export const queryHandlers = [
	GetIngredientQueryHandler,
	GetIngredientsGridQueryHandler,
];
