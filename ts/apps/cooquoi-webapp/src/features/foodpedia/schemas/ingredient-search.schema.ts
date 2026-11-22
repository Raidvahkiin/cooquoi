import { z } from 'zod';

export const INGREDIENT_PAGE_SIZE = 20;

const MAX_QUERY_LENGTH = 100;

/** Sort keys the Q1 resource whitelists, with an optional `-` prefix for descending. */
const sortSchema = z.string().regex(/^-?(relevance|name|code)$/);

const firstValue = (value: unknown) =>
  Array.isArray(value) ? value[0] : value;

// Invalid input falls back to the default instead of failing the page: the URL is user-editable.
const searchSchema = z.object({
  q: z
    .preprocess(firstValue, z.string().trim().max(MAX_QUERY_LENGTH))
    .catch(''),
  page: z.preprocess(firstValue, z.coerce.number().int().min(1)).catch(1),
  sort: z.preprocess(firstValue, sortSchema.optional()).catch(undefined),
});

export type IngredientSearchParams = z.infer<typeof searchSchema>;

/** Parses the page's `searchParams` (`?q=&page=&sort=`); anything invalid becomes its default. */
export function parseIngredientSearchParams(
  searchParams: Record<string, string | string[] | undefined>,
): IngredientSearchParams {
  return searchSchema.parse(searchParams);
}
