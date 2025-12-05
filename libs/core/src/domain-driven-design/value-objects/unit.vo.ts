import { Enumeration } from "../../common";
import { Quantity } from "./quantity.vo";

export abstract class Unit extends Enumeration {
	protected constructor(
		value: string,
		public readonly symbol: string,
		public readonly effective: number,
	) {
		super(value);
	}

	convert(amount: Quantity) {
		return {
			to: (targetUnit: Unit): Quantity => {
				if (!this.isSameMeasurementType(targetUnit)) {
					throw new Error("Cannot convert between different measurement types");
				}
				const valueInBaseUnit = amount.value * this.effective;
				const convertedAmount = valueInBaseUnit / targetUnit.effective;

				return Quantity.create(convertedAmount);
			},
		};
	}

	isSameMeasurementType(unit: Unit): boolean {
		return unit.constructor === this.constructor;
	}
}

export class WeightUnit extends Unit {
	static readonly GRAM = new WeightUnit("gram", "g", 1);
	static readonly KILOGRAM = new WeightUnit("kilogram", "kg", 1000);
	static readonly OUNCE = new WeightUnit("ounce", "oz", 28.3495);
	static readonly POUND = new WeightUnit("pound", "lb", 453.592);

	private constructor(value: string, symbol: string, effective: number) {
		super(value, symbol, effective);
	}
}

export class VolumeUnit extends Unit {
	static readonly LITER = new VolumeUnit("liter", "l", 1);
	static readonly MILLILITER = new VolumeUnit("milliliter", "ml", 0.001);
	static readonly CUP = new VolumeUnit("cup", "cup", 0.24);
	static readonly GALLON = new VolumeUnit("gallon", "gal", 3.78541);

	private constructor(value: string, symbol: string, effective: number) {
		super(value, symbol, effective);
	}
}

export class PieceUnit extends Unit {
	static readonly PIECE = new PieceUnit("piece", "pc", 1);
	static readonly DOZEN = new PieceUnit("dozen", "doz", 12);

	private constructor(value: string, symbol: string, effective: number) {
		super(value, symbol, effective);
	}
}
