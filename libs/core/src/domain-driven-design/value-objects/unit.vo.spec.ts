import { describe, expect, it } from "vitest";
import { Quantity } from "./quantity.vo";
import { PieceUnit, VolumeUnit, WeightUnit } from "./unit.vo";

describe("Unit", () => {
	describe("isSameMeasurementType", () => {
		it("should return true for units of the same type", () => {
			expect(WeightUnit.GRAM.isSameMeasurementType(WeightUnit.KILOGRAM)).toBe(
				true,
			);
		});

		it("should return false for units of different types", () => {
			expect(WeightUnit.GRAM.isSameMeasurementType(VolumeUnit.LITER)).toBe(
				false,
			);
		});
	});

	describe("convert", () => {
		it("should convert from smaller to larger unit", () => {
			const amount = Quantity.create(1000);

			const result = WeightUnit.GRAM.convert(amount).to(WeightUnit.KILOGRAM);

			expect(result.value).toBe(1);
		});

		it("should convert from larger to smaller unit", () => {
			const amount = Quantity.create(1);

			const result = WeightUnit.KILOGRAM.convert(amount).to(WeightUnit.GRAM);

			expect(result.value).toBe(1000);
		});

		it("should convert between non-base units", () => {
			const amount = Quantity.create(1);

			const result = WeightUnit.POUND.convert(amount).to(WeightUnit.OUNCE);

			expect(result.value).toBeCloseTo(16, 0);
		});

		it("should throw error when converting between different measurement types", () => {
			const amount = Quantity.create(100);

			expect(() =>
				WeightUnit.GRAM.convert(amount).to(VolumeUnit.LITER),
			).toThrow("Cannot convert between different measurement types");
		});

		it("should maintain value after round-trip conversion", () => {
			const originalAmount = Quantity.create(500);

			const converted = WeightUnit.GRAM.convert(originalAmount).to(
				WeightUnit.KILOGRAM,
			);
			const backToOriginal = WeightUnit.KILOGRAM.convert(converted).to(
				WeightUnit.GRAM,
			);

			expect(backToOriginal.equals(originalAmount)).toBe(true);
		});
	});
});

describe("WeightUnit", () => {
	it("should have all expected units", () => {
		expect(WeightUnit.GRAM).toBeDefined();
		expect(WeightUnit.KILOGRAM).toBeDefined();
		expect(WeightUnit.OUNCE).toBeDefined();
		expect(WeightUnit.POUND).toBeDefined();
	});
});

describe("VolumeUnit", () => {
	it("should have all expected units", () => {
		expect(VolumeUnit.LITER).toBeDefined();
		expect(VolumeUnit.MILLILITER).toBeDefined();
		expect(VolumeUnit.CUP).toBeDefined();
		expect(VolumeUnit.GALLON).toBeDefined();
	});
});

describe("PieceUnit", () => {
	it("should have all expected units", () => {
		expect(PieceUnit.PIECE).toBeDefined();
		expect(PieceUnit.DOZEN).toBeDefined();
	});
});
