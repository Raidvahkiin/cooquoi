import { Inject } from '@nestjs/common';
import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { eq } from 'drizzle-orm';
import type { NodePgDatabase } from 'drizzle-orm/node-postgres';
import { DATABASE_TOKEN } from '../../../config';
import { type Ingredient, ingredients } from '../../../domain';
import { GetIngredientQuery } from './get-ingredient.query';

@QueryHandler(GetIngredientQuery)
export class GetIngredientHandler
  implements IQueryHandler<GetIngredientQuery, Ingredient | null>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: NodePgDatabase) {}

  async execute(query: GetIngredientQuery): Promise<Ingredient | null> {
    const [row] = await this.db
      .select()
      .from(ingredients)
      .where(eq(ingredients.id, query.id));

    return row ?? null;
  }
}
