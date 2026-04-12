import {
  Body,
  Controller,
  Get,
  NotFoundException,
  Param,
  Post,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { ApiOperation, ApiTags } from '@nestjs/swagger';
import type { Ingredient } from '../domain';
import { CreateIngredientCommand } from './commands/create-ingredient/create-ingredient.command';
import { CreateIngredientDto } from './commands/create-ingredient/create-ingredient.dto';
import { GetIngredientQuery } from './queries/get-ingredient/get-ingredient.query';

@ApiTags('ingredients')
@Controller('ingredients')
export class IngredientController {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

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
