import type { NestApplication } from '@nestjs/core';
import { CqrsModule, QueryBus } from '@nestjs/cqrs';
import { Test } from '@nestjs/testing';
import { DATABASE_TOKEN } from '../../config';
import type { FilterIngredientsResult } from '../../features';
import { FilterIngredientsQuery } from '../../features';
import { MarketModule } from '../../market.module';
import { type PgliteDb, createPgliteDb } from '../helpers/pglite';

describe('FilterIngredients feature', () => {
  let app: NestApplication;
  let db: PgliteDb;
  let queryBus: QueryBus;

  beforeAll(async () => {
    db = await createPgliteDb();

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

    app = module.createNestApplication();
    await app.init();

    queryBus = module.get(QueryBus);
  });

  afterAll(async () => {
    await app.close();
  });

  beforeEach(async () => {
    await db.execute('TRUNCATE TABLE ingredient RESTART IDENTITY');
    await db.execute(`
      INSERT INTO ingredient (name, description) VALUES
        ('salt', 'common seasoning'),
        ('shallot', 'a mild onion'),
        ('salmon', null),
        ('pepper', 'spicy seasoning')
    `);
  });

  it('should returns all ingredients when no search term', async () => {
    const result = await queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(new FilterIngredientsQuery({ skip: 0, take: 20 }));

    expect(result.total).toBe(4);
    expect(result.items).toHaveLength(4);
  });

  it('should find "salt" with fuzzy search "slt"', async () => {
    const result = await queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(new FilterIngredientsQuery({ skip: 0, take: 20, search: 'slt' }));

    const names = result.items.map((i) => i.name);
    expect(names).toContain('salt');
  });

  it('should find both "shallot" and "salmon" with "sl"', async () => {
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
    const result = await queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(new FilterIngredientsQuery({ skip: 0, take: 20, search: 'zzz' }));

    expect(result.total).toBe(0);
    expect(result.items).toHaveLength(0);
  });

  it('should respect skip/take pagination', async () => {
    const result = await queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(new FilterIngredientsQuery({ skip: 0, take: 2 }));

    expect(result.items).toHaveLength(2);
    expect(result.total).toBe(4);
  });

  it('should return correct total even when paginated', async () => {
    const result = await queryBus.execute<
      FilterIngredientsQuery,
      FilterIngredientsResult
    >(new FilterIngredientsQuery({ skip: 3, take: 20 }));

    expect(result.items).toHaveLength(1);
    expect(result.total).toBe(4);
  });
});
