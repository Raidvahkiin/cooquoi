import 'server-only';
import { searchIngredients } from '@cooquoi/api-client';
import { getLocale } from 'next-intl/server';
import { cache } from 'react';
import { type ApiError, createApiClient, toApiError } from '@/shared/api';
import {
  type IngredientSearchViewModel,
  toIngredientSearchViewModel,
} from '../models/ingredient-search.view-model';
import {
  INGREDIENT_PAGE_SIZE,
  type IngredientSearchParams,
} from '../schemas/ingredient-search.schema';

export type IngredientSearchResult =
  | IngredientSearchViewModel
  | { error: ApiError };

/** Q1 in the user's locale. An API problem comes back as `{ error }` (a 401 as `common.unauthorized`, a network failure as `common.unexpected`). */
export const searchIngredientsPage = cache(
  async (params: IngredientSearchParams): Promise<IngredientSearchResult> => {
    const client = await createApiClient();
    const headers = { 'Accept-Language': await getLocale() };
    const search = async (sort: string | undefined) =>
      searchIngredients({
        client,
        headers,
        query: {
          q: params.q || undefined,
          page: params.page,
          pageSize: INGREDIENT_PAGE_SIZE,
          sort: sort ? [sort] : undefined,
        },
      });

    try {
      let result = await search(params.sort);
      // The API's default sort replaces one it rejects.
      if (
        result.error &&
        toApiError(result.error).code === 'common.invalid-sort'
      ) {
        result = await search(undefined);
      }
      // `proxy.ts` refreshes tokens beforehand, so a 401 here is an expired session.
      if (result.response?.status === 401) {
        return { error: { code: 'common.unauthorized' } };
      }
      if (result.error) return { error: toApiError(result.error) };
      return toIngredientSearchViewModel(params, result.data);
    } catch (error) {
      return { error: toApiError(error) };
    }
  },
);
