import { Inject } from '@nestjs/common';
import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import {
  and,
  asc,
  desc,
  eq,
  exists,
  ilike,
  inArray,
  or,
  sql,
} from 'drizzle-orm';
import { DATABASE_TOKEN, type MarketDatabase } from '../../../config';
import {
  type Offer,
  type Product,
  ingredients,
  offers,
  productIngredients,
  products,
} from '../../../domain';
import { FilterProductsQuery } from './filter-products.query';

export type ProductWithOffers = Product & { offers: Offer[] };

export interface FilterProductsResult {
  items: ProductWithOffers[];
  total: number;
}

const SORTABLE_COLUMNS = {
  name: products.name,
  createdAt: products.createdAt,
  updatedAt: products.updatedAt,
} as const;

type SortableField = keyof typeof SORTABLE_COLUMNS;

function fuzzy(term: string): string {
  return `%${term.split('').join('%')}%`;
}

@QueryHandler(FilterProductsQuery)
export class FilterProductsHandler
  implements IQueryHandler<FilterProductsQuery, FilterProductsResult>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: MarketDatabase) {}

  async execute(query: FilterProductsQuery): Promise<FilterProductsResult> {
    const { skip, take, search, maxOffers, sort } = query.params;

    const fuzzyPattern = search ? fuzzy(search) : undefined;

    const nameFilter = fuzzyPattern
      ? ilike(products.name, fuzzyPattern)
      : undefined;

    const ingredientFilter = fuzzyPattern
      ? exists(
          this.db
            .select({ one: sql`1` })
            .from(productIngredients)
            .innerJoin(
              ingredients,
              eq(productIngredients.ingredientId, ingredients.id),
            )
            .where(
              and(
                eq(productIngredients.productId, products.id),
                ilike(ingredients.name, fuzzyPattern),
              ),
            ),
        )
      : undefined;

    // Products matching by name OR by any ingredient name
    const where = fuzzyPattern ? or(nameFilter, ingredientFilter) : undefined;

    // Name matches score 0 (higher priority), ingredient-only matches score 1
    const priorityOrder = fuzzyPattern
      ? asc(
          sql<number>`CASE WHEN ${products.name} ILIKE ${fuzzyPattern} THEN 0 ELSE 1 END`,
        )
      : undefined;

    const secondaryOrder =
      sort && sort.field in SORTABLE_COLUMNS
        ? sort.order === 'desc'
          ? desc(SORTABLE_COLUMNS[sort.field as SortableField])
          : asc(SORTABLE_COLUMNS[sort.field as SortableField])
        : asc(products.name);

    const orderBy = priorityOrder
      ? [priorityOrder, secondaryOrder]
      : [secondaryOrder];

    const [productRows, [{ count }]] = await Promise.all([
      this.db
        .select()
        .from(products)
        .where(where)
        .orderBy(...orderBy)
        .offset(skip)
        .limit(take),
      this.db
        .select({ count: sql<number>`cast(count(*) as int)` })
        .from(products)
        .where(where),
    ]);

    if (productRows.length === 0) {
      return { items: [], total: count };
    }

    const offerRows =
      maxOffers !== undefined
        ? await this.db
            .select()
            .from(offers)
            .where(
              and(
                inArray(
                  offers.productId,
                  productRows.map((p) => p.id),
                ),
                sql`(
                  SELECT COUNT(*) FROM ${offers} o2
                  WHERE o2.product_id = ${offers.productId}
                    AND (CAST(o2.price_amount AS numeric), o2.id::text) < (CAST(${offers.priceAmount} AS numeric), ${offers.id}::text)
                ) < ${maxOffers}`,
              ),
            )
            .orderBy(
              asc(offers.productId),
              asc(sql`CAST(${offers.priceAmount} AS numeric)`),
              asc(offers.id),
            )
        : [];

    const offersByProductId = new Map<string, Offer[]>();
    for (const offer of offerRows) {
      const list = offersByProductId.get(offer.productId) ?? [];
      list.push(offer);
      offersByProductId.set(offer.productId, list);
    }

    const items: ProductWithOffers[] = productRows.map((p) => ({
      ...p,
      offers: offersByProductId.get(p.id) ?? [],
    }));

    return { items, total: count };
  }
}
