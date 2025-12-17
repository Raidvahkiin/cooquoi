import { EntityFilter } from "@libs/core";
import { Ingredient } from "../entities";

export abstract class IngredientRepository {
	abstract save(ingredient: Ingredient): Promise<void>;
	abstract deleteById(id: string): Promise<void>;
	abstract findById(id: string): Promise<Ingredient | null>;
	abstract findMany(filters: EntityFilter<Ingredient>[]): Promise<Ingredient[]>;
}
