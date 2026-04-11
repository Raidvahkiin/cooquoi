import { DatetimeProvider } from "../datetime";
import { mockPartial } from "../test-utils";
import { describe, expect, it, vi } from "vitest";
import { Entity } from "./entity";

class TestEntity extends Entity<TestEntity> {
	constructor(
		private _someProperty: string,
		private _ignoredProp = "ignored",
	) {
		super();
	}
	public get someProperty() {
		return this._someProperty;
	}
	public get ignoredProp() {
		return this._ignoredProp;
	}

	public setState(
		newState: Partial<TestEntity>,
		datetimeProvider: DatetimeProvider,
	) {
		this.commitUpdate({
			newState: newState,
			omitKeys: ["ignoredProp"],
			datetimeProvider,
		});
	}
}

describe("Entity", () => {
	describe("commitUpdate", () => {
		it("should update the entity state", () => {
			// Arrange
			const entity = new TestEntity("entity1");

			const mockDatetimeProvider = mockPartial<DatetimeProvider>({
				now: vi.fn(() => new Date("2024-01-01T00:00:00Z")),
			});

			// Act
			entity.setState({ someProperty: "newValue" }, mockDatetimeProvider);

			// Assert
			expect(entity.someProperty).toBe("newValue");
		});

		it("should update the updatedAt property using the provided DatetimeProvider", () => {
			// Arrange
			const entity = new TestEntity("entity1");
			const expectedUpdatedAt = new Date("2024-01-01T00:00:00Z");

			const mockDatetimeProvider = mockPartial<DatetimeProvider>({
				now: vi.fn(() => expectedUpdatedAt),
			});
			// Act
			entity.setState({ someProperty: "newValue" }, mockDatetimeProvider);

			// Assert
			expect(entity.updatedAt).toEqual(expectedUpdatedAt);
			expect(mockDatetimeProvider.now).toHaveBeenCalled();
		});

		it("should not update updatedAt if no state change occurs", () => {
			// Arrange
			const entity = new TestEntity("entity1");
			const initialUpdatedAt = entity.updatedAt;

			const mockDatetimeProvider = mockPartial<DatetimeProvider>({
				now: vi.fn(),
			});

			// Act
			entity.setState(
				{ someProperty: entity.someProperty }, // same value as before
				mockDatetimeProvider,
			);
			entity.setState({}, mockDatetimeProvider); // undefined value

			// Assert
			expect(entity.updatedAt).toBe(initialUpdatedAt);
			expect(mockDatetimeProvider.now).not.toHaveBeenCalled();
		});

		it("should not update omitted properties", () => {
			// Arrange
			const entity = new TestEntity("entity1");
			const initialUpdatedAt = entity.updatedAt;

			const mockDatetimeProvider = mockPartial<DatetimeProvider>({
				now: vi.fn(),
			});

			// Act
			entity.setState({ ignoredProp: "newIgnoredValue" }, mockDatetimeProvider);

			// Assert
			expect(entity.ignoredProp).not.toBe("newIgnoredValue");
			expect(entity.updatedAt).toBe(initialUpdatedAt);
			expect(mockDatetimeProvider.now).not.toHaveBeenCalled();
		});
	});

	describe("constructor", () => {
		it("should initialize id, createdAt, and updatedAt properties", () => {
			// Arrange
			const id = "entity1";
			const createdAt = new Date("2023-01-01T00:00:00Z");
			const updatedAt = new Date("2023-06-01T00:00:00Z");

			// Act
			const entity = new (class extends Entity<any> {})({
				id,
				createdAt,
				updatedAt,
			});

			// Assert
			expect(entity.id).toBe(id);
			expect(entity.createdAt).toBe(createdAt);
			expect(entity.updatedAt).toBe(updatedAt);
		});

		it("should set id, createdAt and updatedAt to current date if not provided", () => {
			// Arrange
			const id = "entity2";
			const beforeCreation = new Date();

			// Act
			const entity = new (class extends Entity<any> {})({ id });

			const afterCreation = new Date();

			// Assert
			expect(entity.id).toBe(id);
			expect(entity.createdAt.getTime()).toBeGreaterThanOrEqual(
				beforeCreation.getTime(),
			);
			expect(entity.createdAt.getTime()).toBeLessThanOrEqual(
				afterCreation.getTime(),
			);
			expect(entity.updatedAt.getTime()).toBeGreaterThanOrEqual(
				beforeCreation.getTime(),
			);
			expect(entity.updatedAt.getTime()).toBeLessThanOrEqual(
				afterCreation.getTime(),
			);
		});
	});
});
