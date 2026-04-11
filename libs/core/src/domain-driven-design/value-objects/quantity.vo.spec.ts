import { describe, expect, it } from "vitest";
import { Quantity } from "./quantity.vo";

describe("Quantity", () => {
	describe("create", () => {
		it("should create a quantity with a valid value", () => {
			const quantity = Quantity.create(10);

			expect(quantity.value).toBe(10);
		});

		it("should create a quantity with zero value", () => {
			const quantity = Quantity.create(0);

			expect(quantity.value).toBe(0);
		});

		it("should create a quantity with decimal value", () => {
			const quantity = Quantity.create(10.5);

			expect(quantity.value).toBe(10.5);
		});

		it("should throw an error when value is negative", () => {
			expect(() => Quantity.create(-1)).toThrow("Quantity cannot be negative");
		});
	});

	describe("increment", () => {
		it("should increment the quantity by the given amount", () => {
			const quantity = Quantity.create(10);

			const result = quantity.increment(5);

			expect(result.value).toBe(15);
		});

		it("should return a new quantity instance", () => {
			const quantity = Quantity.create(10);

			const result = quantity.increment(5);

			expect(result).not.toBe(quantity);
		});

		it("should throw an error when increment amount is negative", () => {
			const quantity = Quantity.create(10);

			expect(() => quantity.increment(-5)).toThrow(
				"Increment amount cannot be negative",
			);
		});
	});

	describe("decrement", () => {
		it("should decrement the quantity by the given amount", () => {
			const quantity = Quantity.create(10);

			const result = quantity.decrement(5);

			expect(result.value).toBe(5);
		});

		it("should return a new quantity instance", () => {
			const quantity = Quantity.create(10);

			const result = quantity.decrement(5);

			expect(result).not.toBe(quantity);
		});

		it("should return zero when decrement exceeds current value", () => {
			const quantity = Quantity.create(5);

			const result = quantity.decrement(10);

			expect(result.value).toBe(0);
		});

		it("should throw an error when decrement amount is negative", () => {
			const quantity = Quantity.create(10);

			expect(() => quantity.decrement(-5)).toThrow(
				"Decrement amount cannot be negative",
			);
		});
	});

	describe("toString", () => {
		it("should return the string representation of the value", () => {
			const quantity = Quantity.create(10);

			expect(quantity.toString()).toBe("10");
		});

		it("should return decimal string representation", () => {
			const quantity = Quantity.create(10.5);

			expect(quantity.toString()).toBe("10.5");
		});
	});

	describe("equals", () => {
		it("should return true when quantities have the same value", () => {
			const quantity1 = Quantity.create(10);
			const quantity2 = Quantity.create(10);

			expect(quantity1.equals(quantity2)).toBe(true);
		});

		it("should return false when quantities have different values", () => {
			const quantity1 = Quantity.create(10);
			const quantity2 = Quantity.create(20);

			expect(quantity1.equals(quantity2)).toBe(false);
		});

		it("should return false when comparing with null", () => {
			const quantity = Quantity.create(10);

			expect(quantity.equals(null)).toBe(false);
		});

		it("should return false when comparing with undefined", () => {
			const quantity = Quantity.create(10);

			expect(quantity.equals(undefined)).toBe(false);
		});

		it("should return true for values within epsilon tolerance", () => {
			const quantity1 = Quantity.create(10);
			const quantity2 = Quantity.create(10 + 1e-10);

			expect(quantity1.equals(quantity2)).toBe(true);
		});

		it("should return false for values outside epsilon tolerance", () => {
			const quantity1 = Quantity.create(10);
			const quantity2 = Quantity.create(10 + 1e-8);

			expect(quantity1.equals(quantity2)).toBe(false);
		});
	});

	describe("compareTo", () => {
		it("should return 0 when quantities are equal", () => {
			const quantity1 = Quantity.create(10);
			const quantity2 = Quantity.create(10);

			expect(quantity1.compareTo(quantity2)).toBe(0);
		});

		it("should return negative when first quantity is less", () => {
			const quantity1 = Quantity.create(5);
			const quantity2 = Quantity.create(10);

			expect(quantity1.compareTo(quantity2)).toBeLessThan(0);
		});

		it("should return positive when first quantity is greater", () => {
			const quantity1 = Quantity.create(10);
			const quantity2 = Quantity.create(5);

			expect(quantity1.compareTo(quantity2)).toBeGreaterThan(0);
		});

		it("should return 1 when comparing with null", () => {
			const quantity = Quantity.create(10);

			expect(quantity.compareTo(null)).toBe(1);
		});

		it("should return 1 when comparing with undefined", () => {
			const quantity = Quantity.create(10);

			expect(quantity.compareTo(undefined)).toBe(1);
		});

		it("should return 0 for values within epsilon tolerance", () => {
			const quantity1 = Quantity.create(10);
			const quantity2 = Quantity.create(10 + 1e-10);

			expect(quantity1.compareTo(quantity2)).toBe(0);
		});
	});
});
