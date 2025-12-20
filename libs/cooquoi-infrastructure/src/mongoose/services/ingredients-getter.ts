import { FilterQuery, Model } from "mongoose";
import {
  buildSetFilterClause,
  buildTextFilterClause,
} from "@libs/utils-mongoose";
import { Injectable } from "@nestjs/common";
import {
  GetManyOptions,
  GetManyResult,
  SortOption,
  FilterModel,
  SetFilter,
  TextFilter,
} from "@libs/core/common";
import { IngredientModel } from "../models";
import { InjectModel } from "@nestjs/mongoose";

@Injectable()
export class IngredientGetter {
  constructor(
    @InjectModel(IngredientModel.name)
    private readonly ingredientModel: Model<IngredientModel>,
  ) {}

  async getMany({
    skip,
    limit,
    sort,
    filter,
  }: GetManyOptions<IngredientModel>): Promise<GetManyResult<IngredientModel>> {
    const mongoFilter = buildMongoFilterQuery(filter);
    const mongoSort = buildMongoSort(sort);

    const [total, dtos] = await Promise.all([
      this.ingredientModel.countDocuments(mongoFilter).exec(),
      this.ingredientModel
        .find(mongoFilter)
        .sort(mongoSort)
        .skip(skip)
        .limit(limit)
        .exec(),
    ]);

    return {
      data: dtos,
      total,
    };
  }
}

function buildMongoSort(
  sort?: SortOption<IngredientModel>,
): Record<string, 1 | -1> {
  const mongoSort: Record<string, 1 | -1> = {};
  if (sort) {
    mongoSort[sort.value] = sort.direction === "asc" ? 1 : -1;
  }
  return mongoSort;
}

function buildMongoFilterQuery(
  filterModel?: FilterModel<IngredientModel>,
): FilterQuery<IngredientModel> {
  if (!filterModel) return {};

  const clauses: FilterQuery<IngredientModel>[] = [];

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
  return { $and: clauses };
}
