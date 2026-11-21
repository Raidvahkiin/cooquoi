import { Inject } from '@nestjs/common';
import { CommandHandler, type ICommandHandler } from '@nestjs/cqrs';
import { DATABASE_TOKEN, type MarketDatabase } from '../../../config';
import { type Ingredient, ingredients } from '../../../domain';
import { CreateIngredientCommand } from './create-ingredient.command';

@CommandHandler(CreateIngredientCommand)
export class CreateIngredientHandler
  implements ICommandHandler<CreateIngredientCommand, Ingredient>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: MarketDatabase) {}

  async execute(command: CreateIngredientCommand): Promise<Ingredient> {
    const [row] = await this.db
      .insert(ingredients)
      .values({ name: command.name, description: command.description })
      .returning();

    return row;
  }
}
