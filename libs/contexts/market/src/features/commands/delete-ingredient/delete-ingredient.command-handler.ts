import { Inject } from '@nestjs/common';
import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { NodePgDatabase } from 'drizzle-orm/node-postgres';
import { eq } from 'drizzle-orm/sql/expressions/conditions';
import { DATABASE_TOKEN } from '../../../config';
import { ingredient } from '../../../domain';
import { DeleteIngredientCommand } from './delete-ingredient.command';

@CommandHandler(DeleteIngredientCommand)
export class DeleteIngredientCommandHandler
  implements ICommandHandler<DeleteIngredientCommand, void>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: NodePgDatabase) {}

  async execute(command: DeleteIngredientCommand): Promise<void> {
    const { id } = command;
    await this.db.delete(ingredient).where(eq(ingredient.id, id));
  }
}
