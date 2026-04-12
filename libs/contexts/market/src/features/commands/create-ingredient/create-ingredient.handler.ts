import { Inject } from '@nestjs/common';
import { CommandHandler, type ICommandHandler } from '@nestjs/cqrs';
import type { NodePgDatabase } from 'drizzle-orm/node-postgres';
import { DATABASE_TOKEN } from '../../../config';
import { type Ingredient, ingredient } from '../../../domain';
import { CreateIngredientCommand } from './create-ingredient.command';

@CommandHandler(CreateIngredientCommand)
export class CreateIngredientHandler
  implements ICommandHandler<CreateIngredientCommand, Ingredient>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: NodePgDatabase) {}

  async execute(command: CreateIngredientCommand): Promise<Ingredient> {
    const [row] = await this.db
      .insert(ingredient)
      .values({ name: command.name, description: command.description })
      .returning();

    return row;
  }
}
