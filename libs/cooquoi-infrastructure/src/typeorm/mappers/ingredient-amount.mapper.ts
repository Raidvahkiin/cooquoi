import { IngredientAmount } from "@cooquoi/domain";
import { UnitMapper, UnitModel } from "./unit.mapper";

export type IngredientAmountModel = {
	amount: number;
	unit: UnitModel;
};

export const IngredientAmountMapper = {
	fromModel(model: IngredientAmountModel) {
		return {
			toValueObject(): IngredientAmount {
				return IngredientAmount.create(
					model.amount,
					UnitMapper.fromModel(model.unit).toValueObject(),
				);
			},
		};
	},

	fromEntity(vo: IngredientAmount) {
		return {
			toModel(): IngredientAmountModel {
				return {
					amount: vo.amount.value,
					unit: UnitMapper.fromValueObject(vo.unit).toModel(),
				};
			},
		};
	},
};
