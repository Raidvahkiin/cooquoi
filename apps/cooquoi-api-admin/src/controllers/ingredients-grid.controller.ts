import { Body, Controller, HttpCode, HttpStatus, Post } from "@nestjs/common";
import { QueryBus } from "@nestjs/cqrs";
import { ApiBody, ApiTags } from "@nestjs/swagger";
import {
  GetIngredientsGridQuery,
  type IngredientsGridResult,
} from "@cooquoi/presentation";
import {
  AgGridFilterModel,
  AgGridServerSideGetRowsRequestDto,
  AgGridServerSideGetRowsResponseDto,
  AgGridSortModelItemDto,
  IngredientGridRowDto,
} from "../dto";
import { AllFilter, FilterModel, SortDirection, SortOption } from "@libs/core";
import { IngredientModel } from "@cooquoi/infrastructure";

@ApiTags("Ingredients")
@Controller("ingredients")
export class IngredientsGridController {
  constructor(private readonly queryBus: QueryBus) {}

  @Post("grid")
  @HttpCode(HttpStatus.OK)
  @ApiBody({ type: AgGridServerSideGetRowsRequestDto })
  async getRows(
    @Body() dto: AgGridServerSideGetRowsRequestDto,
  ): Promise<AgGridServerSideGetRowsResponseDto<IngredientGridRowDto>> {
    const startRow = Number.isFinite(dto.startRow) ? dto.startRow : 0;
    const endRow = Number.isFinite(dto.endRow) ? dto.endRow : startRow + 100;
    const limit = Math.max(0, endRow - startRow);

    const query = new GetIngredientsGridQuery({
      skip: startRow,
      limit: limit,
      filter: this.buildFilterModel(dto.filterModel),
      sort: this.buildSortModel(dto.sortModel),
    });

    const result = await this.queryBus.execute<
      GetIngredientsGridQuery,
      IngredientsGridResult
    >(query);

    return {
      rows: result.rows.map((row) => IngredientGridRowDto.fromModel(row)),
      lastRow: result.lastRow,
    };
  }

  private buildFilterModel(
    filterModel: AgGridFilterModel | undefined,
  ): FilterModel<IngredientModel> | undefined {
    if (!filterModel) {
      return undefined;
    }

    const filter = Object.entries(filterModel).reduce((acc, [key, value]) => {
      if (key in IngredientModel) {
        acc[key as keyof IngredientModel] = value as AllFilter;
      }
      return acc;
    }, {} as FilterModel<IngredientModel>);

    return filter;
  }

  private buildSortModel(
    sortModel: AgGridSortModelItemDto[] | undefined,
  ): SortOption<IngredientModel> | undefined {
    if (!sortModel) {
      return undefined;
    }

    const firstSort = sortModel.find((item) => item.colId in IngredientModel);
    if (!firstSort) {
      return undefined;
    }

    const sort: SortOption<IngredientModel> = {
      value: firstSort.colId as keyof IngredientModel,
      direction: firstSort.sort as SortDirection,
    };

    return sort;
  }
}
