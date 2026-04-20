import { Inject } from '@nestjs/common';
import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { inArray } from 'drizzle-orm';
import { DATABASE_TOKEN, type MarketDatabase } from '../../../config';
import {
  Product,
  ingredients,
  productIngredients,
  products,
} from '../../../domain';
import { CreateProductCommand } from './create-product.command';

@CommandHandler(CreateProductCommand)
export class CreateProductCommandHandler
  implements ICommandHandler<CreateProductCommand, Product>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: MarketDatabase) {}

  async execute(command: CreateProductCommand): Promise<Product> {
    const { name, description, ingredients: ingredientsIds } = command;

    const existingIngredientIds =
      ingredientsIds.length > 0
        ? (
            await this.db
              .select({ id: ingredients.id })
              .from(ingredients)
              .where(inArray(ingredients.id, ingredientsIds))
          ).map((i) => i.id)
        : [];

    return await this.db.transaction(async (tx) => {
      const [product] = await tx
        .insert(products)
        .values({ name, description })
        .returning();

      if (existingIngredientIds.length > 0) {
        await tx.insert(productIngredients).values(
          existingIngredientIds.map((ingredientId) => ({
            productId: product.id,
            ingredientId,
          })),
        );
      }

      return product;
    });
  }
}
