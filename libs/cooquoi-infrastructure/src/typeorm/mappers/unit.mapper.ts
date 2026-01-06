import { PieceUnit, Unit, VolumeUnit, WeightUnit } from "@libs/core";

export type UnitModel = {
	value: string;
	type: string;
};

export const UnitMapper = {
	fromModel(model: UnitModel) {
		return {
			toValueObject(): Unit {
				switch (model.type) {
					case WeightUnit.name:
						return WeightUnit.fromValue(model.value);
					case VolumeUnit.name:
						return VolumeUnit.fromValue(model.value);
					case PieceUnit.name:
						return PieceUnit.fromValue(model.value);
					default:
						throw new Error(`Unknown unit type: ${model.type}`);
				}
			},
		};
	},

	fromValueObject(vo: Unit) {
		return {
			toModel(): UnitModel {
				return {
					value: vo.value,
					type: vo.constructor.name,
				};
			},
		};
	},
};
