import { Ingredient } from "../entities";

export abstract class IngredientRepository {
	abstract save(ingredient: Ingredient): Promise<void>;
	abstract deleteById(id: string): Promise<void>;
	abstract findById(id: string): Promise<Ingredient | null>;
}
