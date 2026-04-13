import type { FilterIngredientsResult, Ingredient } from '@cooquoi/market';
import {
  CreateIngredientCommand,
  CreateIngredientDto,
  FilterIngredientsDto,
  FilterIngredientsQuery,
  GetIngredientQuery,
} from '@cooquoi/market';
import {
  Body,
  Controller,
  Get,
  NotFoundException,
  Param,
  Post,
  Query,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { ApiOperation, ApiTags } from '@nestjs/swagger';

@ApiTags('ingredients')
@Controller('ingredients')
export class IngredientController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @ApiOperation({
    summary: 'Filter ingredients with pagination and fuzzy search',
  })
  @Get()
  filter(
    @Query() query: FilterIngredientsDto,
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

  @ApiOperation({ summary: 'Create a new ingredient' })
  @Post()
  create(@Body() body: CreateIngredientDto): Promise<Ingredient> {
    return this.commandBus.execute<CreateIngredientCommand, Ingredient>(
      new CreateIngredientCommand(body.name, body.description),
    );
  }

  @ApiOperation({ summary: 'Get an ingredient by ID' })
  @Get(':id')
  async getById(@Param('id') id: string): Promise<Ingredient> {
    const result = await this.queryBus.execute<
      GetIngredientQuery,
      Ingredient | null
    >(new GetIngredientQuery(id));

    if (!result) throw new NotFoundException(`Ingredient ${id} not found`);
    return result;
  }
}
