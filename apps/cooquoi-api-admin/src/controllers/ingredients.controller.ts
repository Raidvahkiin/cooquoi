import {
	Body,
	Controller,
	Delete,
	Get,
	HttpCode,
	HttpStatus,
	NotFoundException,
	Param,
	Post,
	Query,
} from "@nestjs/common";
import { CommandBus, QueryBus } from "@nestjs/cqrs";
import { ApiBody, ApiQuery, ApiTags } from "@nestjs/swagger";
import {
	CreateIngredientCommand,
	DeleteIngredientCommand,
} from "@cooquoi/application";
import {
	GetIngredientQuery,
	GetManyIngredientsQuery,
} from "@cooquoi/presentation";
import { Ingredient } from "@cooquoi/domain";
import { EntityPropsFilter } from "@libs/core";
import {
	CreateIngredientDto,
	GetManyIngredientsQueryDto,
	IngredientResponseDto,
} from "../dto";

@ApiTags("Ingredients")
@Controller("ingredients")
export class IngredientsController {
	constructor(
		private readonly commandBus: CommandBus,
		private readonly queryBus: QueryBus,
	) {}

	@Post()
	@HttpCode(HttpStatus.CREATED)
	@ApiBody({ type: CreateIngredientDto })
	async create(@Body() dto: CreateIngredientDto): Promise<void> {
		console.log("this.commandBus:", this.commandBus);
		const command = new CreateIngredientCommand({
			name: dto.name,
			description: dto.description,
		});

		await this.commandBus.execute(command);
	}

	@Delete(":id")
	@HttpCode(HttpStatus.NO_CONTENT)
	async delete(@Param("id") id: string): Promise<void> {
		const command = new DeleteIngredientCommand({ id });
		await this.commandBus.execute(command);
	}

	@Get(":id")
	async getById(@Param("id") id: string): Promise<IngredientResponseDto> {
		const query = new GetIngredientQuery({ ingredientId: id });
		const ingredient = await this.queryBus.execute<
			GetIngredientQuery,
			Ingredient | null
		>(query);

		if (!ingredient) {
			throw new NotFoundException(`Ingredient with id "${id}" not found`);
		}

		return new IngredientResponseDto(ingredient);
	}

	@Get()
	@ApiQuery({ name: "id", required: false, type: String })
	@ApiQuery({ name: "idIn", required: false, type: String })
	@ApiQuery({ name: "name", required: false, type: String })
	@ApiQuery({ name: "nameIn", required: false, type: String })
	@ApiQuery({ name: "description", required: false, type: String })
	@ApiQuery({ name: "descriptionIn", required: false, type: String })
	async getMany(
		@Query() queryDto: GetManyIngredientsQueryDto,
	): Promise<IngredientResponseDto[]> {
		const splitCsv = (value?: string): string[] | undefined => {
			if (!value) return undefined;
			const parts = value
				.split(",")
				.map((v) => v.trim())
				.filter(Boolean);
			return parts.length > 0 ? parts : undefined;
		};

		const condition: Record<string, unknown> = {};

		// Exact-match filters take precedence over IN filters for the same field.
		if (queryDto.id) {
			condition.id = { value: queryDto.id };
		} else {
			const ids = splitCsv(queryDto.idIn);
			if (ids) {
				condition.id = { value: ids, condition: "in" };
			}
		}

		if (queryDto.name) {
			condition.name = { value: queryDto.name };
		} else {
			const names = splitCsv(queryDto.nameIn);
			if (names) {
				condition.name = { value: names, condition: "in" };
			}
		}

		if (queryDto.description) {
			condition.description = { value: queryDto.description };
		} else {
			const descriptions = splitCsv(queryDto.descriptionIn);
			if (descriptions) {
				condition.description = { value: descriptions, condition: "in" };
			}
		}

		const filters =
			Object.keys(condition).length > 0
				? [
						new EntityPropsFilter<Ingredient>(
							condition as EntityPropsFilter<Ingredient>["condition"],
						),
					]
				: [];

		const query = new GetManyIngredientsQuery({ filters });
		const ingredients = await this.queryBus.execute(query);

		return ingredients.map(
			(ingredient) => new IngredientResponseDto(ingredient),
		);
	}
}
