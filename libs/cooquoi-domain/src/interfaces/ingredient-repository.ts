import { Ingredient } from "../entities";

export abstract class IngredientRepository {
	abstract save(ingredient: Ingredient): Promise<void>;
	abstract findById(id: string): Promise<Ingredient | null>;
	abstract findMany(filters: string[]): Promise<Ingredient[]>; // TODO: rework filters type
}
