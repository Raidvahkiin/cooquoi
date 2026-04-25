import { eq } from 'drizzle-orm';
import { getTestSuit } from '../../../../__tests__/setup';
import { ingredients, products } from '../../../../domain';
import { DeleteIngredientCommand } from '../delete-ingredient.command';

describe('DeleteIngredient feature', () => {
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

  it('should delete an ingredient', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const ingredientToDelete = 'salt';
    const id = (
      await db
        .select()
        .from(ingredients)
        .where(eq(ingredients.name, ingredientToDelete))
    )[0].id;

    // act
    await commandBus.execute(new DeleteIngredientCommand(id));

    // assert
    const rows = await db
      .select()
      .from(ingredients)
      .where(eq(ingredients.id, id));

    expect(rows).toHaveLength(0);
  });

  it('should do nothing if ingredient not found', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const nonExistentId = '00000000-0000-0000-0000-000000000000';

    // act
    await commandBus.execute(new DeleteIngredientCommand(nonExistentId));

    // assert - should not throw and all existing ingredients should remain
    const rows = await db.select().from(ingredients);
    expect(rows).toHaveLength(4);
  });

  it('should not delete products linked to the deleted ingredient', async () => {
    // arrange: Spicy Pepper has only pepper
    const { commandBus, db } = testSuit;
    const [pepper] = await db
      .select()
      .from(ingredients)
      .where(eq(ingredients.name, 'pepper'));

    // act
    await commandBus.execute(new DeleteIngredientCommand(pepper.id));

    // assert: ingredient is gone but product remains
    const remainingIngredients = await db
      .select()
      .from(ingredients)
      .where(eq(ingredients.id, pepper.id));
    expect(remainingIngredients).toHaveLength(0);

    const remainingProducts = await db
      .select()
      .from(products)
      .where(eq(products.name, 'Spicy Pepper'));
    expect(remainingProducts).toHaveLength(1);
  });
});
