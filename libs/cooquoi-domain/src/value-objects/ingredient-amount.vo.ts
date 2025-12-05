import { Quantity, Unit, ValueObject } from "@libs/core";
import { InvalidOperationError } from "../errors";

export class IngredientAmount implements ValueObject {
	public readonly amount: Quantity;
	public readonly unit: Unit;

	private constructor(props: { value: Quantity; unit: Unit }) {
		this.amount = props.value;
		this.unit = props.unit;
	}

	static create(value: number, unit: Unit): IngredientAmount {
		return new IngredientAmount({ value: Quantity.create(value), unit });
	}

	toString(): string {
		return `${this.amount}(${this.unit.symbol})`;
	}

	equals(vo: ValueObject | undefined | null): boolean {
		if (!vo || !(vo instanceof IngredientAmount)) {
			return false;
		}

		if (!this.unit.isSameMeasurementType(vo.unit)) {
			return false;
		}

		const convertedAmount = vo.unit.convert(vo.amount).to(this.unit);

		return this.amount.equals(convertedAmount);
	}

	compareTo(vo: ValueObject | undefined | null): number {
		if (!vo || !(vo instanceof IngredientAmount)) {
			throw new InvalidOperationError(
				"Cannot compare IngredientAmount with non-IngredientAmount",
			);
		}

		if (!this.unit.isSameMeasurementType(vo.unit)) {
			throw new InvalidOperationError(
				"Cannot compare IngredientAmount with different measurement types",
			);
		}

		const convertedAmount = vo.unit.convert(vo.amount).to(this.unit);

		return this.amount.compareTo(convertedAmount);
	}
}
