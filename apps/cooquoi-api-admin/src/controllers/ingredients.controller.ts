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
} from "@nestjs/common";
import { CommandBus, QueryBus } from "@nestjs/cqrs";
import { ApiBody, ApiTags } from "@nestjs/swagger";
import {
	CreateIngredientCommand,
	DeleteIngredientCommand,
} from "@cooquoi/application";
import {
	GetIngredientQuery,
	GetManyIngredientsQuery,
} from "@cooquoi/presentation";
import { Ingredient } from "@cooquoi/domain";
import { CreateIngredientDto, IngredientResponseDto } from "../dto";

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
	async getMany(): Promise<IngredientResponseDto[]> {
		const query = new GetManyIngredientsQuery({ filters: [] });
		const ingredients = await this.queryBus.execute<
			GetManyIngredientsQuery,
			Ingredient[]
		>(query);

		return ingredients.map(
			(ingredient) => new IngredientResponseDto(ingredient),
		);
	}
}
