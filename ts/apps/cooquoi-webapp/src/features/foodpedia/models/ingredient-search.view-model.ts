import type { FoodpediaIngredientSummary } from '@cooquoi/api-client';
import type { IngredientSearchParams } from '../schemas/ingredient-search.schema';

export type SortKey = 'relevance' | 'name' | 'code';
type SortDirection = 'ascending' | 'descending';

type IngredientRow = Pick<
  FoodpediaIngredientSummary,
  'id' | 'code' | 'name' | 'baseUnit'
>;

export type IngredientSearchViewModel = {
  query: string;
  page: number;
  totalPages: number;
  totalCount: number;
  sort: { key: SortKey; direction: SortDirection };
  rows: IngredientRow[];
};

type SearchResponse = {
  items: FoodpediaIngredientSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
};

/** The sort the API applies when none is asked for: by relevance with a query, by name without. */
export function toEffectiveSort({
  q,
  sort,
}: Pick<
  IngredientSearchParams,
  'q' | 'sort'
>): IngredientSearchViewModel['sort'] {
  const first = sort ?? (q ? '-relevance' : 'name');
  return {
    key: first.replace(/^-/, '') as SortKey,
    direction: first.startsWith('-') ? 'descending' : 'ascending',
  };
}

export function toIngredientSearchViewModel(
  params: IngredientSearchParams,
  response: SearchResponse,
): IngredientSearchViewModel {
  return {
    query: params.q,
    page: response.page,
    totalPages: Math.max(1, Math.ceil(response.totalCount / response.pageSize)),
    totalCount: response.totalCount,
    sort: toEffectiveSort(params),
    rows: response.items.map(({ id, code, name, baseUnit }) => ({
      id,
      code,
      name,
      baseUnit,
    })),
  };
}

/** URL of a search state; defaults are left out so links stay short. */
export function toSearchHref({
  q,
  page,
  sort,
}: Partial<IngredientSearchParams>): string {
  const search = new URLSearchParams();
  if (q) search.set('q', q);
  if (sort) search.set('sort', sort);
  if (page && page > 1) search.set('page', String(page));
  const queryString = search.toString();
  return queryString ? `/foodpedia?${queryString}` : '/foodpedia';
}

/** Sort asked by clicking a column: ascending first, then flipping. */
export function toNextSort({
  current,
  key,
}: {
  current: IngredientSearchViewModel['sort'];
  key: Exclude<SortKey, 'relevance'>;
}): string {
  return current.key === key && current.direction === 'ascending'
    ? `-${key}`
    : key;
}
