import {
	IngredientModel,
	InjectModel,
	Model,
	type FilterQuery,
} from "@cooquoi/infrastructure";
import { IQueryHandler, Query, QueryHandler } from "@nestjs/cqrs";
import {
	buildSetFilterClause,
	buildTextFilterClause,
} from "./grid-mongo-filters";

export type SortDirection = "asc" | "desc";

export type GridSortModelItem = {
	colId: string;
	sort: SortDirection;
};

export type GridFilterModel = Record<string, unknown>;

export type IngredientsGridRow = {
	_id: string;
	name: string;
	description: string | null;
	createdAt: Date;
	updatedAt: Date;
};

export type IngredientsGridResult = {
	rows: IngredientsGridRow[];
	lastRow: number;
};

export class GetIngredientsGridQuery extends Query<IngredientsGridResult> {
	public readonly startRow: number;
	public readonly endRow: number;
	public readonly sortModel?: GridSortModelItem[];
	public readonly filterModel?: GridFilterModel;

	constructor(props: {
		startRow: number;
		endRow: number;
		sortModel?: GridSortModelItem[];
		filterModel?: GridFilterModel;
	}) {
		super();
		this.startRow = props.startRow;
		this.endRow = props.endRow;
		this.sortModel = props.sortModel;
		this.filterModel = props.filterModel;
	}
}

type TextFilter = {
	filterType?: "text";
	type?: string;
	filter?: unknown;
};

type SetFilter = {
	filterType?: "set";
	values?: unknown;
};

@QueryHandler(GetIngredientsGridQuery)
export class GetIngredientsGridQueryHandler
	implements IQueryHandler<GetIngredientsGridQuery>
{
	constructor(
		@InjectModel(IngredientModel.name)
		private readonly ingredientModel: Model<IngredientModel>,
	) {}

	async execute(
		query: GetIngredientsGridQuery,
	): Promise<IngredientsGridResult> {
		const startRow = Number.isFinite(query.startRow) ? query.startRow : 0;
		const endRow = Number.isFinite(query.endRow)
			? query.endRow
			: startRow + 100;
		const limit = Math.max(0, endRow - startRow);

		const mongoFilter = this.buildMongoFilter(query.filterModel);
		const mongoSort = this.buildMongoSort(query.sortModel);

		const [total, models] = await Promise.all([
			this.ingredientModel.countDocuments(mongoFilter).exec(),
			this.ingredientModel
				.find(mongoFilter)
				.sort(mongoSort)
				.skip(startRow)
				.limit(limit)
				.lean<IngredientsGridRow[]>()
				.exec(),
		]);

		return {
			rows: models,
			lastRow: total,
		};
	}

	private buildMongoSort(
		sortModel?: GridSortModelItem[],
	): Record<string, 1 | -1> {
		const first = sortModel?.[0];
		if (!first) return {};

		const field = this.mapColIdToModelPath(first.colId);
		if (!field) return {};

		return { [field]: first.sort === "desc" ? -1 : 1 };
	}

	private buildMongoFilter(
		filterModel?: GridFilterModel,
	): FilterQuery<IngredientModel> {
		if (!filterModel) return {};

		const clauses: FilterQuery<IngredientModel>[] = [];

		for (const [colId, raw] of Object.entries(filterModel)) {
			const field = this.mapColIdToModelPath(colId);
			if (!field) continue;

			const text = raw as TextFilter;
			if (text?.filterType === "text") {
				const clause = buildTextFilterClause<IngredientModel>(
					field,
					text.type,
					text.filter,
				);
				if (clause) {
					clauses.push(clause);
					continue;
				}
			}

			const set = raw as SetFilter;
			if (set?.filterType === "set") {
				const clause = buildSetFilterClause<IngredientModel>(field, set.values);
				if (clause) clauses.push(clause);
			}
		}

		if (clauses.length === 0) return {};
		if (clauses.length === 1) return clauses[0];
		return { $and: clauses };
	}

	private mapColIdToModelPath(colId: string): string | null {
		// Whitelist supported columns for grid filtering/sorting.
		switch (colId) {
			case "id":
				return "_id";
			case "name":
				return "name";
			case "description":
				return "description";
			case "createdAt":
				return "createdAt";
			case "updatedAt":
				return "updatedAt";
			default:
				return null;
		}
	}
}
