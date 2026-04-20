import { CommandBus, CqrsModule, QueryBus } from '@nestjs/cqrs';
import { Test } from '@nestjs/testing';
import { DATABASE_TOKEN } from '../../config';
import { ingredients, offers } from '../../domain';
import { products } from '../../domain/entities/product.entity';
import { productIngredients } from '../../domain/entities/relations';
import { MarketModule } from '../../market.module';
import { PgliteDb, createPgliteDb } from './pglite';

export async function getTestSuit() {
  const db: PgliteDb = await createPgliteDb();

  const module = await Test.createTestingModule({
    imports: [
      CqrsModule,
      MarketModule.register({
        database: { url: 'postgresql://localhost/test' },
      }),
    ],
  })
    .overrideProvider(DATABASE_TOKEN)
    .useValue(db)
    .compile();

  const app = module.createNestApplication();
  await app.init();

  const queryBus = module.get(QueryBus);
  const commandBus = module.get(CommandBus);

  async function clearDb() {
    await db.execute(
      'TRUNCATE TABLE product_ingredients, offers, ingredients, products CASCADE',
    );
  }

  async function resetDb() {
    await clearDb();

    await db.insert(ingredients).values([
      { name: 'salt', description: 'common seasoning' },
      { name: 'shallot', description: 'a mild onion' },
      { name: 'salmon', description: null },
      { name: 'pepper', description: 'spicy seasoning' },
    ]);

    const [superSalmon, greaterSalmon, spicyPepper] = await db
      .insert(products)
      .values([
        { name: 'Super Salmon', description: 'Delicious salmon product' },
        { name: 'Greater Salmon', description: 'Exquisite salmon product' },
        { name: 'Spicy Pepper', description: 'Hot and spicy pepper product' },
      ])
      .returning();

    const insertedIngredients = await db.select().from(ingredients);
    const byName = Object.fromEntries(
      insertedIngredients.map((i) => [i.name, i]),
    );

    await db.insert(productIngredients).values([
      { productId: superSalmon.id, ingredientId: byName.salmon.id },
      { productId: superSalmon.id, ingredientId: byName.salt.id },
      { productId: greaterSalmon.id, ingredientId: byName.salmon.id },
      { productId: greaterSalmon.id, ingredientId: byName.shallot.id },
      { productId: spicyPepper.id, ingredientId: byName.pepper.id },
    ]);

    await db.insert(offers).values([
      {
        productId: superSalmon.id,
        vendor: 'VendorA',
        priceAmount: '12.99',
        priceCurrency: 'EUR',
      },
      {
        productId: superSalmon.id,
        vendor: 'VendorB',
        priceAmount: '9.99',
        priceCurrency: 'EUR',
      },
      {
        productId: superSalmon.id,
        vendor: 'VendorC',
        priceAmount: '15.00',
        priceCurrency: 'EUR',
      },
      {
        productId: greaterSalmon.id,
        vendor: 'VendorA',
        priceAmount: '8.50',
        priceCurrency: 'EUR',
      },
      {
        productId: greaterSalmon.id,
        vendor: 'VendorB',
        priceAmount: '11.00',
        priceCurrency: 'EUR',
      },
    ]);
  }

  return { app, db, module, commandBus, queryBus, clearDb, resetDb };
}
