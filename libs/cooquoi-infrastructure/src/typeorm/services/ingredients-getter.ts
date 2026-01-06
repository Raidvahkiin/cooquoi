import {
	FilterModel,
	GetManyOptions,
	GetManyResult,
	SetFilter,
	SortOption,
	TextFilter,
} from "@libs/core/common";
import { Injectable } from "@nestjs/common";
import { InjectRepository } from "@nestjs/typeorm";
import { FindOptionsWhere, In, Like, Not, Raw, Repository } from "typeorm";
import { IngredientModel } from "../models";

@Injectable()
export class IngredientGetter {
	constructor(
		@InjectRepository(IngredientModel)
		private readonly ingredientRepo: Repository<IngredientModel>,
	) {}

	async getMany({
		skip,
		limit,
		sort,
		filter,
	}: GetManyOptions<IngredientModel>): Promise<GetManyResult<IngredientModel>> {
		const typeormFilter = buildTypeormFilterQuery(filter);
		const typeormSort = buildTypeormSort(sort);

		const [total, dtos] = await Promise.all([
			this.ingredientRepo.count({
				where: typeormFilter,
			}),
			this.ingredientRepo.find({
				skip,
				take: limit,
				where: typeormFilter,
				order: typeormSort,
			}),
		]);

		return {
			data: dtos,
			total,
		};
	}
}

function buildTypeormFilterQuery(
	filterModel?: FilterModel<IngredientModel>,
): FindOptionsWhere<IngredientModel> {
	if (!filterModel) return {};

	const clauses: FindOptionsWhere<IngredientModel>[] = [];

	for (const [field, filter] of Object.entries(filterModel)) {
		if (!field || !filter) continue;

		switch (filter.filterType) {
			case "text": {
				const textFilter = filter as TextFilter;
				const clause = buildTextFilterClause<IngredientModel>(
					field,
					textFilter.type,
					textFilter.filter,
				);
				if (clause) {
					clauses.push(clause);
				}
				break;
			}
			case "set": {
				const setFilter = filter as SetFilter;
				const clause = buildSetFilterClause<IngredientModel>(
					field,
					setFilter.values,
				);
				if (clause) {
					clauses.push(clause);
				}
				break;
			}
		}
	}

	if (clauses.length === 0) return {};
	if (clauses.length === 1) return clauses[0];

	// For TypeORM, multiple conditions in the same object are ANDed together
	return Object.assign({}, ...clauses);
}

function buildTypeormSort(
	sort?: SortOption<IngredientModel>,
): Record<string, "ASC" | "DESC"> {
	const typeormSort: Record<string, "ASC" | "DESC"> = {};
	if (sort) {
		typeormSort[sort.value] = sort.direction === "asc" ? "ASC" : "DESC";
	}
	return typeormSort;
}

function buildTextFilterClause<TModel>(
	field: string,
	type: string | undefined,
	filter: unknown,
): FindOptionsWhere<TModel> | null {
	if (typeof filter !== "string") return null;

	const rawValue = filter.trim();
	if (rawValue.length === 0) return null;

	const value = buildTextFilterValue(type, rawValue);
	if (value === undefined) return null;

	return {
		[field]: value,
	} as FindOptionsWhere<TModel>;
}

function buildSetFilterClause<TModel>(
	field: string,
	values: unknown,
): FindOptionsWhere<TModel> | null {
	const list = Array.isArray(values)
		? values.filter((v) => typeof v === "string")
		: [];
	if (list.length === 0) return null;

	return {
		[field]: In(list),
	} as FindOptionsWhere<TModel>;
}

function buildTextFilterValue(type: string | undefined, filterValue: string) {
	if (!filterValue) return undefined;

	switch (type) {
		case "equals":
			return filterValue;
		case "notEqual":
			return Not(filterValue);
		case "contains":
			return Like(`%${filterValue}%`);
		case "notContains":
			return Not(Like(`%${filterValue}%`));
		case "startsWith":
			return Like(`${filterValue}%`);
		case "endsWith":
			return Like(`%${filterValue}`);
		case "fuzzy": {
			// Improved fuzzy matching that handles multi-word strings
			// Strategy:
			// 1. Split search into words
			// 2. For each word, check if it appears in the target with small edit distance
			// 3. Use word boundaries to match parts of multi-word strings
			const searchWords = filterValue
				.toLowerCase()
				.split(/\s+/)
				.filter((w) => w.length > 0);

			if (searchWords.length === 0) return undefined;

			// For single word search, use combination of:
			// - Word boundary matching (e.g., "egg" matches "egg yolk")
			// - Levenshtein for typos (e.g., "eg" matches "egg")
			if (searchWords.length === 1) {
				const word = searchWords[0];
				const maxDistance = Math.max(2, Math.floor(word.length * 0.3)); // Allow 30% error

				return Raw(
					(alias) =>
						`(LOWER(${alias}) LIKE LOWER(:wordPattern) OR levenshtein(LOWER(${alias}), LOWER(:searchValue)) <= :maxDistance)`,
					{
						searchValue: filterValue,
						wordPattern: `%${word}%`, // Word appears anywhere in the string
						maxDistance,
					},
				);
			}

			// For multiple words, all words must match (AND condition)
			return Raw(
				(alias) => {
					const conditions = searchWords
						.map((_, idx) => `LOWER(${alias}) LIKE LOWER(:word${idx})`)
						.join(" AND ");
					return conditions;
				},
				Object.fromEntries(
					searchWords.map((word, idx) => [`word${idx}`, `%${word}%`]),
				),
			);
		}
		default:
			return undefined;
	}
}
