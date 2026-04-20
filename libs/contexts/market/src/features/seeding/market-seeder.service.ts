import { Inject, Injectable, Logger } from '@nestjs/common';
import { sql } from 'drizzle-orm';
import { DATABASE_TOKEN, type MarketDatabase } from '../../config';
import {
  ingredients,
  offers,
  productIngredients,
  products,
} from '../../domain';
import type { SeedData } from './market-seeder.types';

@Injectable()
export class MarketSeederService {
  private readonly logger = new Logger(MarketSeederService.name);

  constructor(@Inject(DATABASE_TOKEN) private readonly db: MarketDatabase) {}

  async seed(data: SeedData): Promise<void> {
    this.logger.log('Resetting database...');
    await this.db.execute(
      sql`TRUNCATE TABLE product_ingredients, offers, ingredients, products CASCADE`,
    );

    this.logger.log(`Seeding ${data.ingredients.length} ingredients...`);
    const insertedIngredients = await this.db
      .insert(ingredients)
      .values(data.ingredients)
      .returning();

    const ingredientByName = new Map(
      insertedIngredients.map((i) => [i.name, i]),
    );

    this.logger.log(`Seeding ${data.products.length} products...`);
    for (const seedProduct of data.products) {
      const insertedProducts = await this.db
        .insert(products)
        .values({
          name: seedProduct.name,
          description: seedProduct.description,
        })
        .returning();

      const product = insertedProducts[0];

      const linkedIngredientIds = seedProduct.ingredients
        .map((name) => ingredientByName.get(name)?.id)
        .filter((id): id is string => id !== undefined);

      if (linkedIngredientIds.length > 0) {
        await this.db.insert(productIngredients).values(
          linkedIngredientIds.map((ingredientId) => ({
            productId: product.id,
            ingredientId,
          })),
        );
      }

      if (seedProduct.offers.length > 0) {
        await this.db.insert(offers).values(
          seedProduct.offers.map((o) => ({
            productId: product.id,
            vendor: o.vendor,
            priceAmount: o.priceAmount,
            priceCurrency: o.priceCurrency,
          })),
        );
      }
    }

    this.logger.log('Seeding complete.');
  }
}
