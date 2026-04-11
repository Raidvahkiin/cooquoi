import { Entity } from "./entity";

export abstract class EntityFilter<T extends Entity<T>> {
	constructor(public readonly operator: "AND" | "OR" = "AND") {}
}

type EntityPropCondition<V> =
	| {
			value: V;
			condition?: Exclude<EntityPropOperator, "in" | "nin">;
	  }
	| {
			value: V[];
			condition: Extract<EntityPropOperator, "in" | "nin">;
	  };

export class EntityPropsFilter<T extends Entity<T>> extends EntityFilter<T> {
	constructor(
		public readonly condition: Partial<{
			[K in keyof T]: EntityPropCondition<T[K]>;
		}>,
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
