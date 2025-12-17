import { Ingredient } from "@cooquoi/domain";
import { EntityFilterProcessor } from "@libs/utils-mongoose";
import { IngredientModel } from "./models/ingredient.model";

export class IngredientFilterProcessor extends EntityFilterProcessor<
	Ingredient,
	IngredientModel
> {
	protected mapEntityPropToModelPath(prop: keyof Ingredient): string {
		if (prop === "id") {
			return "_id";
		}
		return prop as string;
	}
}
