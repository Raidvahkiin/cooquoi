import { Entity } from "./entity";

export abstract class EntityFilter<T extends Entity<T>> {
	constructor(public readonly operator: "AND" | "OR" = "AND") {}
}

export class EntityPropsFilter<T extends Entity<T>> extends EntityFilter<T> {
	constructor(
		public readonly condition: Record<
			keyof T,
			{
				value: T[keyof T];
				condition?: EntityPropOperator;
			}
		>,
		operator?: "AND" | "OR",
	) {
		super(operator);
	}
}

export class GroupedEntityFilter<T extends Entity<T>> extends EntityFilter<T> {
	constructor(
		public readonly filters: EntityFilter<T>[],
		operator?: "AND" | "OR",
	) {
		super(operator);
	}
}

export type EntityPropOperator = "eq" | "neq" | "in" | "nin";
