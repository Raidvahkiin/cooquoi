'use client';

import { Pagination } from '@astryxdesign/core/Pagination';
import { useTranslations } from 'next-intl';
import { toSearchHref } from '../../models/ingredient-search.view-model';

type IngredientPaginationProps = {
  query: string;
  sort?: string;
  page: number;
  totalPages: number;
};

/** Page links that keep `q` and `sort`; navigation is the links', so `onChange` has nothing to do. */
export function IngredientPagination({
  query,
  sort,
  page,
  totalPages,
}: IngredientPaginationProps) {
  const t = useTranslations('foodpedia.pagination');
  return (
    <Pagination
      label={t('label')}
      page={page}
      totalPages={totalPages}
      onChange={() => {}}
      getPageHref={(target) => toSearchHref({ q: query, sort, page: target })}
    />
  );
}
