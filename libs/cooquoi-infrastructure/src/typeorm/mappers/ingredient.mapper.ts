import { Ingredient } from "@cooquoi/domain";
import { IngredientModel } from "../models";

export const IngredientMapper = {
	fromModel(model: IngredientModel) {
		return {
			toEntity(): Ingredient {
				return new Ingredient({
					id: model.id,
					name: model.name,
					description: model.description,
					createdAt: model.createdAt,
					updatedAt: model.updatedAt,
				});
			},
		};
	},

	fromEntity(entity: Ingredient) {
		return {
			toModel(): IngredientModel {
				return {
					id: entity.id,
					name: entity.name,
					description: entity.description ?? undefined,
					createdAt: entity.createdAt,
					updatedAt: entity.updatedAt,
				};
			},
		};
	},
};
