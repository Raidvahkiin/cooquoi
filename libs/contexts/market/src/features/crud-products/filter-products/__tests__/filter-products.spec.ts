import type { FilterProductsResult } from '../../..';
import { FilterProductsQuery } from '../../..';
import { getTestSuit } from '../../../../__tests__/setup';
import { ingredients, productIngredients, products } from '../../../../domain';

describe('FilterProducts feature', () => {
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

  it('should return all products when no filters', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 20 }));

    expect(result.total).toBe(3);
    expect(result.items).toHaveLength(3);
  });

  it('should fuzzy match product name "salm" -> Super Salmon, Greater Salmon', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 20, search: 'salm' }));

    const names = result.items.map((p) => p.name);
    expect(names).toContain('Super Salmon');
    expect(names).toContain('Greater Salmon');
    expect(names).not.toContain('Spicy Pepper');
  });

  it('should fuzzy match product name "sprslm" -> Super Salmon only', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 20, search: 'sprslm' }));

    const names = result.items.map((p) => p.name);
    expect(names).toContain('Super Salmon');
    expect(names).not.toContain('Greater Salmon');
  });

  it('should return empty when search has no match', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 20, search: 'zzz' }));

    expect(result.total).toBe(0);
    expect(result.items).toHaveLength(0);
  });

  it('should find products when search matches ingredient name only', async () => {
    const { queryBus } = testSuit;
    // "slt" (%s%l%t%) matches "salt" and "shallot" but no product name
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 20, search: 'slt' }));

    const names = result.items.map((p) => p.name);
    expect(names).toContain('Super Salmon'); // has salt
    expect(names).toContain('Greater Salmon'); // has shallot
    expect(names).not.toContain('Spicy Pepper');
  });

  it('should not return duplicate products when multiple ingredients match', async () => {
    const { queryBus } = testSuit;
    // Both Super Salmon and Greater Salmon have the "salmon" ingredient
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 20, search: 'salm' }));

    const names = result.items.map((p) => p.name);
    const uniqueNames = [...new Set(names)];
    expect(names).toHaveLength(uniqueNames.length);
  });

  it('should rank name matches before ingredient-only matches', async () => {
    const { db, clearDb, queryBus } = testSuit;
    await clearDb();

    // Product whose name matches "salmon"
    const [nameMatchProduct] = await db
      .insert(products)
      .values({ name: 'Salmon Fillet', description: null })
      .returning();

    // Product whose name does NOT match "salmon", but has salmon as ingredient
    const [ingredientOnlyProduct] = await db
      .insert(products)
      .values({ name: 'Beef Stew', description: null })
      .returning();

    const [salmonIngredient] = await db
      .insert(ingredients)
      .values({ name: 'salmon', description: null })
      .returning();

    await db.insert(productIngredients).values({
      productId: ingredientOnlyProduct.id,
      ingredientId: salmonIngredient.id,
    });

    void nameMatchProduct; // referenced above

    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 20, search: 'salmon' }));

    expect(result.total).toBe(2);
    expect(result.items[0].name).toBe('Salmon Fillet'); // name match → first
    expect(result.items[1].name).toBe('Beef Stew'); // ingredient-only → second
  });

  it('should respect skip/take pagination', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 2 }));

    expect(result.items).toHaveLength(2);
    expect(result.total).toBe(3);
  });

  it('should return correct total even when paginated', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 2, take: 20 }));

    expect(result.items).toHaveLength(1);
    expect(result.total).toBe(3);
  });

  it('should sort by name desc', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(
      new FilterProductsQuery({
        skip: 0,
        take: 20,
        sort: { field: 'name', order: 'desc' },
      }),
    );

    const names = result.items.map((p) => p.name);
    expect(names).toEqual([...names].sort((a, b) => b.localeCompare(a)));
  });

  it('should return empty offers array when maxOffers not specified', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 20 }));

    for (const item of result.items) {
      expect(item.offers).toEqual([]);
    }
  });

  it('should return top-N offers per product sorted by lowest price when maxOffers is set', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 20, maxOffers: 2 }));

    const superSalmon = result.items.find((p) => p.name === 'Super Salmon');
    const greaterSalmon = result.items.find((p) => p.name === 'Greater Salmon');
    const spicyPepper = result.items.find((p) => p.name === 'Spicy Pepper');

    // Super Salmon has 3 offers (9.99, 12.99, 15.00) -> top 2: 9.99, 12.99
    expect(superSalmon?.offers).toHaveLength(2);
    expect(superSalmon?.offers[0].priceAmount).toBe('9.99');
    expect(superSalmon?.offers[1].priceAmount).toBe('12.99');

    // Greater Salmon has 2 offers (8.50, 11.00) -> top 2: both
    expect(greaterSalmon?.offers).toHaveLength(2);
    expect(greaterSalmon?.offers[0].priceAmount).toBe('8.50');
    expect(greaterSalmon?.offers[1].priceAmount).toBe('11.00');

    // Spicy Pepper has no offers
    expect(spicyPepper?.offers).toEqual([]);
  });

  it('should return all offers when maxOffers exceeds available offers', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 20, maxOffers: 10 }));

    const superSalmon = result.items.find((p) => p.name === 'Super Salmon');
    expect(superSalmon?.offers).toHaveLength(3);
  });

  it('should return exactly 1 offer (cheapest) when maxOffers=1', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterProductsQuery,
      FilterProductsResult
    >(new FilterProductsQuery({ skip: 0, take: 20, maxOffers: 1 }));

    const superSalmon = result.items.find((p) => p.name === 'Super Salmon');
    expect(superSalmon?.offers).toHaveLength(1);
    expect(superSalmon?.offers[0].priceAmount).toBe('9.99');
  });
});
