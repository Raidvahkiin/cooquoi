import { eq } from 'drizzle-orm';
import { getTestSuit } from '../../../../__tests__/setup';
import { offers, products } from '../../../../domain';
import { DeleteProductCommand } from '../delete-product.command';

describe('DeleteProduct feature', () => {
  let testSuit: Awaited<ReturnType<typeof getTestSuit>>;

  beforeAll(async () => {
    testSuit = await getTestSuit();
  });

  afterAll(async () => {
    await testSuit?.app?.close();
  });

  beforeEach(async () => {
    const { resetDb } = testSuit;
    await resetDb();
  });

  it('should delete a product', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const [product] = await db
      .select()
      .from(products)
      .where(eq(products.name, 'Spicy Pepper'));

    // act
    await commandBus.execute(new DeleteProductCommand(product.id));

    // assert
    const rows = await db
      .select()
      .from(products)
      .where(eq(products.id, product.id));
    expect(rows).toHaveLength(0);
  });

  it('should cascade delete offers when product is deleted', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const [product] = await db
      .select()
      .from(products)
      .where(eq(products.name, 'Super Salmon'));
    const offersBefore = await db
      .select()
      .from(offers)
      .where(eq(offers.productId, product.id));
    expect(offersBefore.length).toBeGreaterThan(0);

    // act
    await commandBus.execute(new DeleteProductCommand(product.id));

    // assert
    const offersAfter = await db
      .select()
      .from(offers)
      .where(eq(offers.productId, product.id));
    expect(offersAfter).toHaveLength(0);
  });

  it('should do nothing if product not found', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const nonExistentId = '00000000-0000-0000-0000-000000000000';
    const totalBefore = (await db.select().from(products)).length;

    // act
    await commandBus.execute(new DeleteProductCommand(nonExistentId));

    // assert
    const rows = await db.select().from(products);
    expect(rows).toHaveLength(totalBefore);
  });
});
