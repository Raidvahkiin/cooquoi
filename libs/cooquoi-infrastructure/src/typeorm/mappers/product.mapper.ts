import { Product } from "@cooquoi/domain";
import {
	IngredientModel,
	ProductIngredientQuantity,
	ProductModel,
} from "../models";
import {
	IngredientAmountMapper,
	IngredientAmountModel,
} from "./ingredient-amount.mapper";
import { IngredientMapper } from "./ingredient.mapper";

export const ProductMapper = {
	fromModel(model: ProductModel) {
		return {
			toEntity(): Product {
				// Map ingredients with their quantities
				const ingredientsMap = new Map();

				for (const qty of model.ingredientQuantities) {
					const ingredient = model.ingredients.find(
						(ing) => ing.id === qty.ingredientId,
					);
					if (ingredient) {
						const ingredientAmountModel: IngredientAmountModel = {
							amount: qty.quantityAmount,
							unit: {
								type: qty.quantityUnitType,
								value: qty.quantityUnitValue,
							},
						};
						ingredientsMap.set(
							IngredientMapper.fromModel(ingredient).toEntity(),
							IngredientAmountMapper.fromModel(
								ingredientAmountModel,
							).toValueObject(),
						);
					}
				}

				return new Product({
					id: model.id,
					createdAt: model.createdAt,
					updatedAt: model.updatedAt,
					name: model.name,
					aliases: model.aliases,
					ingredients: ingredientsMap,
					customAttributes: model.customAttributes,
				});
			},
		};
	},

	fromEntity(entity: Product) {
		return {
			toModel(): ProductModel {
				const ingredientQuantities: ProductIngredientQuantity[] = [];
				const ingredients: IngredientModel[] = [];

				for (const [ingredient, quantity] of entity.ingredients.entries()) {
					ingredientQuantities.push({
						ingredientId: ingredient.id,
						quantityAmount: quantity.amount.value,
						quantityUnitType: quantity.unit.constructor.name,
						quantityUnitValue: quantity.unit.value,
					});
					ingredients.push(IngredientMapper.fromEntity(ingredient).toModel());
				}

				return {
					id: entity.id,
					createdAt: entity.createdAt,
					updatedAt: entity.updatedAt,
					name: entity.name,
					aliases: entity.aliases,
					ingredientQuantities,
					customAttributes: entity.customAttributes,
					ingredients,
				};
			},
		};
	},
};
