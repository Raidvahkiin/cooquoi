import { Inject } from '@nestjs/common';
import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { ilike, sql } from 'drizzle-orm';
import type { NodePgDatabase } from 'drizzle-orm/node-postgres';
import { DATABASE_TOKEN } from '../../../config';
import { type Ingredient, ingredient } from '../../../domain';
import { FilterIngredientsQuery } from './filter-ingredients.query';

export interface FilterIngredientsResult {
  items: Ingredient[];
  total: number;
}

@QueryHandler(FilterIngredientsQuery)
export class FilterIngredientsHandler
  implements IQueryHandler<FilterIngredientsQuery, FilterIngredientsResult>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: NodePgDatabase) {}

  async execute(
    query: FilterIngredientsQuery,
  ): Promise<FilterIngredientsResult> {
    const { skip, take, search } = query.params;
    const where = search
      ? ilike(ingredient.name, `%${search.split('').join('%')}%`)
      : undefined;

    const [items, [{ count }]] = await Promise.all([
      this.db.select().from(ingredient).where(where).offset(skip).limit(take),
      this.db
        .select({ count: sql<number>`cast(count(*) as int)` })
        .from(ingredient)
        .where(where),
    ]);

    return { items, total: count };
  }
}
