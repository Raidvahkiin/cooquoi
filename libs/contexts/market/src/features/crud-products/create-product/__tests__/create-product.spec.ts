import { eq } from 'drizzle-orm';
import { ingredients, productIngredients, products } from '../../../../domain';
import { CreateProductCommand } from '../create-product.command';
import { getTestSuit } from '../../../../__tests__/setup';

describe('CreateProduct feature', () => {
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

  it('should create a product without ingredients', async () => {
    // arrange
    const { commandBus, db } = testSuit;

    // act
    const product = await commandBus.execute(
      new CreateProductCommand({ name: 'Plain Product', ingredients: [] }),
    );

    // assert
    expect(product).toMatchObject({ name: 'Plain Product' });

    const rows = await db
      .select()
      .from(products)
      .where(eq(products.id, product.id));
    expect(rows).toHaveLength(1);

    const links = await db
      .select()
      .from(productIngredients)
      .where(eq(productIngredients.productId, product.id));
    expect(links).toHaveLength(0);
  });

  it('should create a product and link existing ingredients', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const existingIngredients = await db.select().from(ingredients);
    // biome-ignore lint/style/noNonNullAssertion: test setup guarantees these exist
    const salt = existingIngredients.find((i) => i.name === 'salt')!;
    // biome-ignore lint/style/noNonNullAssertion: test setup guarantees these exist
    const pepper = existingIngredients.find((i) => i.name === 'pepper')!;

    // act
    const product = await commandBus.execute(
      new CreateProductCommand({
        name: 'Salted Pepper',
        description: 'A salty pepper product',
        ingredients: [salt.id, pepper.id],
      }),
    );

    // assert
    expect(product).toMatchObject({ name: 'Salted Pepper' });

    const links = await db
      .select()
      .from(productIngredients)
      .where(eq(productIngredients.productId, product.id));
    expect(links).toHaveLength(2);
    expect(links.map((l) => l.ingredientId)).toEqual(
      expect.arrayContaining([salt.id, pepper.id]),
    );
  });

  it('should skip non-existing ingredient ids and only link existing ones', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const existingIngredients = await db.select().from(ingredients);
    // biome-ignore lint/style/noNonNullAssertion: test setup guarantees these exist
    const salt = existingIngredients.find((i) => i.name === 'salt')!;
    const nonExistentId = '00000000-0000-0000-0000-000000000000';

    // act
    const product = await commandBus.execute(
      new CreateProductCommand({
        name: 'Partial Product',
        ingredients: [salt.id, nonExistentId],
      }),
    );

    // assert
    const links = await db
      .select()
      .from(productIngredients)
      .where(eq(productIngredients.productId, product.id));
    expect(links).toHaveLength(1);
    expect(links[0].ingredientId).toBe(salt.id);
  });

  it('should create a product with no links when all ingredient ids are non-existing', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const nonExistentId = '00000000-0000-0000-0000-000000000000';

    // act
    const product = await commandBus.execute(
      new CreateProductCommand({
        name: 'Ghost Product',
        ingredients: [nonExistentId],
      }),
    );

    // assert
    expect(product).toMatchObject({ name: 'Ghost Product' });

    const links = await db
      .select()
      .from(productIngredients)
      .where(eq(productIngredients.productId, product.id));
    expect(links).toHaveLength(0);
  });
});
