import { Body, Controller, HttpCode, HttpStatus, Post } from "@nestjs/common";
import { QueryBus } from "@nestjs/cqrs";
import { ApiBody, ApiTags } from "@nestjs/swagger";
import {
	GetIngredientsGridQuery,
	type IngredientsGridResult,
} from "@cooquoi/presentation";
import {
	AgGridServerSideGetRowsRequestDto,
	AgGridServerSideGetRowsResponseDto,
	IngredientGridRowDto,
} from "../dto";

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
		const query = new GetIngredientsGridQuery({
			startRow: dto.startRow,
			endRow: dto.endRow,
			sortModel: dto.sortModel,
			filterModel: dto.filterModel,
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
}
