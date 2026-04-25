import { Inject } from '@nestjs/common';
import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { eq } from 'drizzle-orm/sql/expressions/conditions';
import { DATABASE_TOKEN, type MarketDatabase } from '../../../config';
import { ingredients } from '../../../domain';
import { DeleteIngredientCommand } from './delete-ingredient.command';

@CommandHandler(DeleteIngredientCommand)
export class DeleteIngredientCommandHandler
  implements ICommandHandler<DeleteIngredientCommand, void>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: MarketDatabase) {}

  async execute(command: DeleteIngredientCommand): Promise<void> {
    const { id } = command;

    // Delete the ingredient (cascades to product_ingredients rows)
    await this.db.delete(ingredients).where(eq(ingredients.id, id));
  }
}
