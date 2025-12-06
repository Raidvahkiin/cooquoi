import { GetIngredientQueryHandler } from "./get-ingredient.query";
import { GetManyIngredientsQueryHandler } from "./get-many-ingredients.query";

export * from "./get-ingredient.query";
export * from "./get-many-ingredients.query";

export const queryHandlers = [
	GetIngredientQueryHandler,
	GetManyIngredientsQueryHandler,
];
