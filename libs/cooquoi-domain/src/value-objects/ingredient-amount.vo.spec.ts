import { VolumeUnit, WeightUnit } from "@libs/core";
import { describe, expect, it } from "vitest";
import { InvalidOperationError } from "../errors";
import { IngredientAmount } from "./ingredient-amount.vo";

describe("IngredientAmount", () => {
	describe("create", () => {
		it("should create an ingredient amount with valid value and unit", () => {
			const ingredientAmount = IngredientAmount.create(100, WeightUnit.GRAM);

			expect(ingredientAmount.amount.value).toBe(100);
			expect(ingredientAmount.unit).toBe(WeightUnit.GRAM);
		});

		it("should throw error when value is negative", () => {
			expect(() => IngredientAmount.create(-1, WeightUnit.GRAM)).toThrow(
				"Quantity cannot be negative",
			);
		});

		it("should create with zero value", () => {
			const ingredientAmount = IngredientAmount.create(0, WeightUnit.GRAM);

			expect(ingredientAmount.amount.value).toBe(0);
		});
	});

	describe("toString", () => {
		it("should return formatted string with value and unit symbol", () => {
			const ingredientAmount = IngredientAmount.create(100, WeightUnit.GRAM);

			expect(ingredientAmount.toString()).toBe("100(g)");
		});

		it("should handle decimal values", () => {
			const ingredientAmount = IngredientAmount.create(
				1.5,
				WeightUnit.KILOGRAM,
			);

			expect(ingredientAmount.toString()).toBe("1.5(kg)");
		});
	});

	describe("equals", () => {
		it("should return true for same value and unit", () => {
			const amount1 = IngredientAmount.create(100, WeightUnit.GRAM);
			const amount2 = IngredientAmount.create(100, WeightUnit.GRAM);

			expect(amount1.equals(amount2)).toBe(true);
		});

		it("should return true for equivalent amounts in different units", () => {
			const amount1 = IngredientAmount.create(1000, WeightUnit.GRAM);
			const amount2 = IngredientAmount.create(1, WeightUnit.KILOGRAM);

			expect(amount1.equals(amount2)).toBe(true);
		});

		it("should return false for different amounts", () => {
			const amount1 = IngredientAmount.create(100, WeightUnit.GRAM);
			const amount2 = IngredientAmount.create(200, WeightUnit.GRAM);

			expect(amount1.equals(amount2)).toBe(false);
		});

		it("should return false for different measurement types", () => {
			const amount1 = IngredientAmount.create(100, WeightUnit.GRAM);
			const amount2 = IngredientAmount.create(100, VolumeUnit.MILLILITER);

			expect(amount1.equals(amount2)).toBe(false);
		});

		it("should return false when comparing with null", () => {
			const amount = IngredientAmount.create(100, WeightUnit.GRAM);

			expect(amount.equals(null)).toBe(false);
		});

		it("should return false when comparing with undefined", () => {
			const amount = IngredientAmount.create(100, WeightUnit.GRAM);

			expect(amount.equals(undefined)).toBe(false);
		});
	});

	describe("compareTo", () => {
		it("should return 0 for equal amounts", () => {
			const amount1 = IngredientAmount.create(100, WeightUnit.GRAM);
			const amount2 = IngredientAmount.create(100, WeightUnit.GRAM);

			expect(amount1.compareTo(amount2)).toBe(0);
		});

		it("should return 0 for equivalent amounts in different units", () => {
			const amount1 = IngredientAmount.create(1000, WeightUnit.GRAM);
			const amount2 = IngredientAmount.create(1, WeightUnit.KILOGRAM);

			expect(amount1.compareTo(amount2)).toBe(0);
		});

		it("should return negative when first amount is less", () => {
			const amount1 = IngredientAmount.create(100, WeightUnit.GRAM);
			const amount2 = IngredientAmount.create(200, WeightUnit.GRAM);

			expect(amount1.compareTo(amount2)).toBeLessThan(0);
		});

		it("should return positive when first amount is greater", () => {
			const amount1 = IngredientAmount.create(200, WeightUnit.GRAM);
			const amount2 = IngredientAmount.create(100, WeightUnit.GRAM);

			expect(amount1.compareTo(amount2)).toBeGreaterThan(0);
		});

		it("should compare correctly across different units", () => {
			const amount1 = IngredientAmount.create(500, WeightUnit.GRAM);
			const amount2 = IngredientAmount.create(1, WeightUnit.KILOGRAM);

			expect(amount1.compareTo(amount2)).toBeLessThan(0);
		});

		it("should throw InvalidOperationError when comparing with null", () => {
			const amount = IngredientAmount.create(100, WeightUnit.GRAM);

			expect(() => amount.compareTo(null)).toThrow(InvalidOperationError);
			expect(() => amount.compareTo(null)).toThrow(
				"Cannot compare IngredientAmount with non-IngredientAmount",
			);
		});

		it("should throw InvalidOperationError when comparing with undefined", () => {
			const amount = IngredientAmount.create(100, WeightUnit.GRAM);

			expect(() => amount.compareTo(undefined)).toThrow(InvalidOperationError);
		});

		it("should throw InvalidOperationError when comparing different measurement types", () => {
			const amount1 = IngredientAmount.create(100, WeightUnit.GRAM);
			const amount2 = IngredientAmount.create(100, VolumeUnit.MILLILITER);

			expect(() => amount1.compareTo(amount2)).toThrow(InvalidOperationError);
			expect(() => amount1.compareTo(amount2)).toThrow(
				"Cannot compare IngredientAmount with different measurement types",
			);
		});
	});
});
