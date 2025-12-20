import { describe, expect, it } from "vitest";
import { Entity, EntityPropsFilter, GroupedEntityFilter } from "@libs/core";
import { EntityFilterProcessor } from "./entity-filter-processor";

class TestEntity extends Entity<TestEntity> {
	private _name: string;
	private _age: number;

	constructor(
		props: { name: string; age: number } & { id?: string } = {
			name: "n",
			age: 1,
		},
	) {
		super({ id: props.id });
		this._name = props.name;
		this._age = props.age;
	}

	get name() {
		return this._name;
	}

	get age() {
		return this._age;
	}
}

type TestModel = {
	_id: string;
	name: string;
	age: number;
};

class TestProcessor extends EntityFilterProcessor<TestEntity, TestModel> {
	protected mapEntityPropToModelPath(prop: keyof TestEntity): string {
		return prop === "id" ? "_id" : (prop as string);
	}
}

describe("EntityFilterProcessor", () => {
	it("maps id -> _id and eq condition", () => {
		const processor = new TestProcessor();
		const query = processor.process([
			new EntityPropsFilter<TestEntity>({
				id: { value: "abc" },
			}),
		]);

		expect(query).toEqual({ _id: "abc" });
	});

	it("supports OR between sequential filters", () => {
		const processor = new TestProcessor();
		const query = processor.process([
			new EntityPropsFilter<TestEntity>({ name: { value: "a" } }),
			new EntityPropsFilter<TestEntity>({ name: { value: "b" } }, "OR"),
		]);

		expect(query).toEqual({ $or: [{ name: "a" }, { name: "b" }] });
	});

	it("processes grouped filters", () => {
		const processor = new TestProcessor();
		const group = new GroupedEntityFilter<TestEntity>([
			new EntityPropsFilter<TestEntity>({ name: { value: "a" } }),
			new EntityPropsFilter<TestEntity>({ name: { value: "b" } }, "OR"),
		]);

		const query = processor.process([group]);
		expect(query).toEqual({ $or: [{ name: "a" }, { name: "b" }] });
	});

	it("supports in/nin operators", () => {
		const processor = new TestProcessor();
		const query = processor.process([
			new EntityPropsFilter<TestEntity>({
				age: { value: [1, 2, 3], condition: "in" },
			}),
		]);

		expect(query).toEqual({ age: { $in: [1, 2, 3] } });
	});
});
