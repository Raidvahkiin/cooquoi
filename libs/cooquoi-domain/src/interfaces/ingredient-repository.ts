import { Ingredient } from "../entities";

export abstract class IngredientRepository {
	abstract save(ingredient: Ingredient): Promise<void>;
}
