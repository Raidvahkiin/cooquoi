import { CreateIngredientCommandHandler } from "./create-ingredient";
import { CreateProductCommandHandler } from "./create-product";
import { DeleteIngredientCommandHandler } from "./delete-ingredient";

export * from "./create-ingredient";
export * from "./create-product";
export * from "./delete-ingredient";

export const commandHandlers = [
	CreateIngredientCommandHandler,
	DeleteIngredientCommandHandler,
	CreateProductCommandHandler,
];
