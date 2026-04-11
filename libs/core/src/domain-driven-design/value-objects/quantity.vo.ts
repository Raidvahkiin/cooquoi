import { ValueObject } from "./value-object";

export class Quantity implements ValueObject {
	private static readonly EPSILON = 1e-9;

	private constructor(public readonly value: number) {}

	static create(value: number): Quantity {
		if (value < 0) {
			throw new Error("Quantity cannot be negative");
		}
		return new Quantity(value);
	}

	increment(amount: number): Quantity {
		if (amount < 0) {
			throw new Error("Increment amount cannot be negative");
		}
		return new Quantity(this.value + amount);
	}

	decrement(amount: number): Quantity {
		if (amount < 0) {
			throw new Error("Decrement amount cannot be negative");
		}
		const newValue = this.value - amount;
		return new Quantity(newValue < 0 ? 0 : newValue);
	}

	multiply(factor: number): Quantity {
		if (factor < 0) {
			throw new Error("Multiply factor cannot be negative");
		}
		return new Quantity(this.value * factor);
	}

	toString(): string {
		return this.value.toString();
	}

	equals(vo: ValueObject | undefined | null): boolean {
		if (vo === null || vo === undefined) {
			return false;
		}
		if (!(vo instanceof Quantity)) {
			return false;
		}
		return Math.abs(this.value - vo.value) < Quantity.EPSILON;
	}

	compareTo(vo: ValueObject | undefined | null): number {
		if (vo === null || vo === undefined) {
			return 1;
		}
		if (!(vo instanceof Quantity)) {
			return 1;
		}
		if (Math.abs(this.value - vo.value) < Quantity.EPSILON) {
			return 0;
		}
		return this.value - vo.value;
	}
}
