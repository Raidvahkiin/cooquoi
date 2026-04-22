import { Inject } from '@nestjs/common';
import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { inArray } from 'drizzle-orm';
import { eq } from 'drizzle-orm/sql/expressions/conditions';
import { DATABASE_TOKEN, type MarketDatabase } from '../../../config';
import { ingredients, productIngredients, products } from '../../../domain';
import { DeleteIngredientCommand } from './delete-ingredient.command';

@CommandHandler(DeleteIngredientCommand)
export class DeleteIngredientCommandHandler
  implements ICommandHandler<DeleteIngredientCommand, void>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: MarketDatabase) {}

  async execute(command: DeleteIngredientCommand): Promise<void> {
    const { id } = command;

    await this.db.transaction(async (tx) => {
      // Find products linked to this ingredient before deleting it
      const affectedProductIds = (
        await tx
          .select({ productId: productIngredients.productId })
          .from(productIngredients)
          .where(eq(productIngredients.ingredientId, id))
      ).map((r) => r.productId);

      // Delete the ingredient (cascades to product_ingredients rows)
      await tx.delete(ingredients).where(eq(ingredients.id, id));

      if (affectedProductIds.length > 0) {
        // Find which affected products still have at least one ingredient
        const stillLinked = await tx
          .select({ productId: productIngredients.productId })
          .from(productIngredients)
          .where(inArray(productIngredients.productId, affectedProductIds));

        const stillLinkedIds = new Set(stillLinked.map((r) => r.productId));
        const orphanedIds = affectedProductIds.filter(
          (pid) => !stillLinkedIds.has(pid),
        );

        if (orphanedIds.length > 0) {
          await tx.delete(products).where(inArray(products.id, orphanedIds));
        }
      }
    });
  }
}
