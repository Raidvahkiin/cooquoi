import type { FilterIngredientsResult } from '../../..';
import { FilterIngredientsQuery } from '../../..';
import { getTestSuit } from '../../../../__tests__/setup';

describe('FilterIngredients feature', () => {
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

  it('should returns all ingredients when no search term', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(new FilterIngredientsQuery({ skip: 0, take: 20 }));

    expect(result.total).toBe(4);
    expect(result.items).toHaveLength(4);
  });

  it('should find "salt" with fuzzy search "slt"', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(new FilterIngredientsQuery({ skip: 0, take: 20, search: 'slt' }));

    const names = result.items.map((i) => i.name);
    expect(names).toContain('salt');
  });

  it('should find both "shallot" and "salmon" with "sl"', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(new FilterIngredientsQuery({ skip: 0, take: 20, search: 'sl' }));

    const names = result.items.map((i) => i.name);
    expect(names).toContain('shallot');
    expect(names).toContain('salmon');
    expect(names).not.toContain('pepper');
  });

  it('should return empty when no match', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(new FilterIngredientsQuery({ skip: 0, take: 20, search: 'zzz' }));

    expect(result.total).toBe(0);
    expect(result.items).toHaveLength(0);
  });

  it('should respect skip/take pagination', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(new FilterIngredientsQuery({ skip: 0, take: 2 }));

    expect(result.items).toHaveLength(2);
    expect(result.total).toBe(4);
  });

  it('should return correct total even when paginated', async () => {
    const { queryBus } = testSuit;
    const result = await queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(new FilterIngredientsQuery({ skip: 3, take: 20 }));

    expect(result.items).toHaveLength(1);
    expect(result.total).toBe(4);
  });
});
