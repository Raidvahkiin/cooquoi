import { CreateIngredientCommandHandler } from "./create-ingredient";
import { DeleteIngredientCommandHandler } from "./delete-ingredient";

export * from "./create-ingredient";
export * from "./delete-ingredient";

export const commandHandlers = [
	CreateIngredientCommandHandler,
	DeleteIngredientCommandHandler,
];
