import type { FilterIngredientsResult } from '@cooquoi/market';
import {
  CreateIngredientCommand,
  DeleteIngredientCommand,
  FilterIngredientsQuery,
  GetIngredientQuery,
} from '@cooquoi/market';
import {
  Body,
  Controller,
  Delete,
  Get,
  NotFoundException,
  Param,
  Post,
  Query,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { GetIngredientsRequestDto } from './dtos/get-ingredients-request.dto';
import type {
  CreateIngredientBody,
  CreateIngredientResponse,
  Ingredient,
  IngredientsEndpoints,
} from '@utils/api-contracts/cooquoi';

@Controller('ingredients')
export class IngredientsController implements IngredientsEndpoints {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Post()
  async create(
    @Body() body: CreateIngredientBody,
  ): Promise<CreateIngredientResponse> {
    const ingredient = await this.commandBus.execute<
      CreateIngredientCommand,
      Ingredient
    >(new CreateIngredientCommand(body.name, body.description));
    return { id: ingredient.id };
  }

  @Get(':id')
  async getOneById(@Param('id') id: string): Promise<Ingredient> {
    const result = await this.queryBus.execute<
      GetIngredientQuery,
      Ingredient | null
    >(new GetIngredientQuery(id));

    if (!result) throw new NotFoundException(`Ingredient ${id} not found`);
    return result;
  }

  @Get()
  getMany(
    @Query() query: GetIngredientsRequestDto,
  ): Promise<FilterIngredientsResult> {
    return this.queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(
      new FilterIngredientsQuery({
        skip: query.skip ?? 0,
        take: query.take ?? 20,
        search: query.search,
        sort: query.sortField
          ? { field: query.sortField, order: query.sortOrder ?? 'asc' }
          : undefined,
      }),
    );
  }

  @Delete(':id')
  async delete(@Param('id') id: string): Promise<void> {
    await this.commandBus.execute(new DeleteIngredientCommand(id));
  }
}
