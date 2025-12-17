import {
	Entity,
	EntityFilter,
	EntityPropsFilter,
	EntityPropOperator,
	GroupedEntityFilter,
} from "@libs/core";
import type { FilterQuery } from "mongoose";

type LogicalOp = "AND" | "OR";

export abstract class EntityFilterProcessor<
	TEntity extends Entity<TEntity>,
	TModel extends object,
> {
	/**
	 * Maps an entity property (domain) to a model path (Mongo).
	 * Example: `id` -> `_id`.
	 */
	protected abstract mapEntityPropToModelPath(prop: keyof TEntity): string;

	/**
	 * Hook to map/serialize entity values for storage (e.g. ValueObjects).
	 */
	protected mapEntityValueToModelValue(
		_prop: keyof TEntity,
		value: unknown,
	): unknown {
		return value;
	}

	process(filters: EntityFilter<TEntity>[]): FilterQuery<TModel> {
		return this.buildFromFilters(filters);
	}

	private buildFromFilters(
		filters: EntityFilter<TEntity>[],
	): FilterQuery<TModel> {
		let acc: FilterQuery<TModel> | null = null;

		for (const filter of filters) {
			const expr = this.buildFromFilter(filter);
			if (this.isEmptyExpr(expr)) {
				continue;
			}

			if (!acc) {
				acc = expr;
				continue;
			}

			acc = this.combine(acc, expr, filter.operator);
		}

		return acc ?? {};
	}

	private buildFromFilter(filter: EntityFilter<TEntity>): FilterQuery<TModel> {
		if (this.isGroupedEntityFilter(filter)) {
			return this.buildFromFilters(filter.filters);
		}

		if (this.isEntityPropsFilter(filter)) {
			return this.buildFromProps(filter.condition, filter.operator);
		}

		return {};
	}

	private isGroupedEntityFilter(
		filter: EntityFilter<TEntity>,
	): filter is GroupedEntityFilter<TEntity> {
		return filter instanceof GroupedEntityFilter;
	}

	private isEntityPropsFilter(
		filter: EntityFilter<TEntity>,
	): filter is EntityPropsFilter<TEntity> {
		return filter instanceof EntityPropsFilter;
	}

	private buildFromProps(
		condition: EntityPropsFilter<TEntity>["condition"],
		operator: LogicalOp,
	): FilterQuery<TModel> {
		const clauses: FilterQuery<TModel>[] = [];

		for (const [rawKey, entry] of Object.entries(condition)) {
			const entityProp = rawKey as keyof TEntity;
			if (typeof entityProp !== "string") {
				continue;
			}

			const modelPath = this.mapEntityPropToModelPath(entityProp);
			const mappedValue = this.mapEntityValueToModelValue(
				entityProp,
				(entry as { value: unknown; condition?: EntityPropOperator }).value,
			);

			if (mappedValue === undefined) {
				continue;
			}

			const op =
				(entry as { value: unknown; condition?: EntityPropOperator })
					.condition ?? "eq";

			clauses.push(this.buildLeaf(modelPath, mappedValue, op));
		}

		if (clauses.length === 0) {
			return {};
		}

		if (clauses.length === 1) {
			return clauses[0];
		}

		return operator === "OR" ? { $or: clauses } : { $and: clauses };
	}

	private buildLeaf(
		modelPath: string,
		value: unknown,
		operator: EntityPropOperator,
	): FilterQuery<TModel> {
		switch (operator) {
			case "eq":
				return { [modelPath]: value } as FilterQuery<TModel>;
			case "neq":
				return { [modelPath]: { $ne: value } } as FilterQuery<TModel>;
			case "in":
				return {
					[modelPath]: { $in: Array.isArray(value) ? value : [value] },
				} as FilterQuery<TModel>;
			case "nin":
				return {
					[modelPath]: { $nin: Array.isArray(value) ? value : [value] },
				} as FilterQuery<TModel>;
			default:
				return { [modelPath]: value } as FilterQuery<TModel>;
		}
	}

	private combine(
		left: FilterQuery<TModel>,
		right: FilterQuery<TModel>,
		op: LogicalOp,
	): FilterQuery<TModel> {
		return op === "OR" ? this.or(left, right) : this.and(left, right);
	}

	private and(
		left: FilterQuery<TModel>,
		right: FilterQuery<TModel>,
	): FilterQuery<TModel> {
		if (this.isEmptyExpr(left)) return right;
		if (this.isEmptyExpr(right)) return left;

		const leftAnd = (left as { $and?: FilterQuery<TModel>[] }).$and;
		const rightAnd = (right as { $and?: FilterQuery<TModel>[] }).$and;

		if (leftAnd && rightAnd) {
			return { $and: [...leftAnd, ...rightAnd] };
		}
		if (leftAnd) {
			return { $and: [...leftAnd, right] };
		}
		if (rightAnd) {
			return { $and: [left, ...rightAnd] };
		}
		return { $and: [left, right] };
	}

	private or(
		left: FilterQuery<TModel>,
		right: FilterQuery<TModel>,
	): FilterQuery<TModel> {
		if (this.isEmptyExpr(left)) return right;
		if (this.isEmptyExpr(right)) return left;

		const leftOr = (left as { $or?: FilterQuery<TModel>[] }).$or;
		const rightOr = (right as { $or?: FilterQuery<TModel>[] }).$or;

		if (leftOr && rightOr) {
			return { $or: [...leftOr, ...rightOr] };
		}
		if (leftOr) {
			return { $or: [...leftOr, right] };
		}
		if (rightOr) {
			return { $or: [left, ...rightOr] };
		}
		return { $or: [left, right] };
	}

	private isEmptyExpr(expr: FilterQuery<TModel> | null | undefined): boolean {
		if (!expr) return true;
		return Object.keys(expr as object).length === 0;
	}
}
