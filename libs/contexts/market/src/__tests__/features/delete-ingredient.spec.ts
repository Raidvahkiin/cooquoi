import { eq } from 'drizzle-orm';
import { ingredient } from '../../domain';
import { DeleteIngredientCommand } from '../../features';
import { getTestSuit } from '../setup';

describe('DeleteIngredient feature', () => {
  let testSuit: Awaited<ReturnType<typeof getTestSuit>>;

  beforeAll(async () => {
    testSuit = await getTestSuit();
  });

  afterAll(async () => {
    await testSuit?.app?.close();
  });

  beforeEach(async () => {
    const { db, clearDb } = testSuit;

    // seed some data for testing
    await clearDb();
    await db.execute(`
      INSERT INTO ingredient (name, description) VALUES
        ('salt', 'common seasoning'),
        ('shallot', 'a mild onion'),
        ('salmon', null),
        ('pepper', 'spicy seasoning')
    `);
  });

  it('should delete an ingredient', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const ingredientToDelete = 'salt';
    const id = (
      await db
        .select()
        .from(ingredient)
        .where(eq(ingredient.name, ingredientToDelete))
    )[0].id;

    // act
    await commandBus.execute(new DeleteIngredientCommand(id));

    // assert
    const rows = await db
      .select()
      .from(ingredient)
      .where(eq(ingredient.id, id));

    expect(rows).toHaveLength(0);
  });

  it('should do nothing if ingredient not found', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const nonExistentId = '00000000-0000-0000-0000-000000000000';

    // act
    await commandBus.execute(new DeleteIngredientCommand(nonExistentId));

    // assert - should not throw and all existing ingredients should remain
    const rows = await db.select().from(ingredient);
    expect(rows).toHaveLength(4);
  });
});
