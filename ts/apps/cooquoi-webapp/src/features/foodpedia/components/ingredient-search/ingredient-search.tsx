import { EmptyState } from '@astryxdesign/core/EmptyState';
import { Heading } from '@astryxdesign/core/Heading';
import { Link } from '@astryxdesign/core/Link';
import { VStack } from '@astryxdesign/core/Stack';
import {
  Table,
  TableBody,
  TableCell,
  TableHeader,
  TableHeaderCell,
  TableRow,
} from '@astryxdesign/core/Table';
import { useTranslations } from 'next-intl';
import { ErrorMessage } from '@/shared/ui';
import {
  type IngredientSearchViewModel,
  type SortKey,
  toNextSort,
  toSearchHref,
} from '../../models/ingredient-search.view-model';
import type { IngredientSearchResult } from '../../queries/search-ingredients.query';
import { IngredientPagination } from './ingredient-pagination';
import { IngredientSearchForm } from './ingredient-search-form';

type IngredientSearchProps = {
  result: IngredientSearchResult;
  /** The `sort` of the URL, if any: only an explicit sort is carried over to links. */
  sort?: string;
};

/** Foodpedia search (Q1): query form, sortable table, pagination; all state is in the URL. */
export function IngredientSearch({ result, sort }: IngredientSearchProps) {
  const t = useTranslations('foodpedia');
  const query = 'error' in result ? '' : result.query;
  return (
    <VStack gap={4}>
      <Heading level={1}>{t('title')}</Heading>
      <IngredientSearchForm key={query} query={query} sort={sort} />
      {'error' in result ? (
        <ErrorMessage error={result.error} />
      ) : (
        <Results search={result} sort={sort} />
      )}
    </VStack>
  );
}

function Results({
  search,
  sort,
}: {
  search: IngredientSearchViewModel;
  sort?: string;
}) {
  const t = useTranslations('foodpedia');
  if (search.rows.length === 0) {
    return (
      <EmptyState
        title={t('empty.title')}
        description={t('empty.description')}
      />
    );
  }
  return (
    <VStack gap={3}>
      <Table aria-label={t('table.label')}>
        <TableHeader>
          <TableRow>
            <SortableHeader search={search} sortKey="name" />
            <SortableHeader search={search} sortKey="code" />
            <TableHeaderCell scope="col">{t('table.baseUnit')}</TableHeaderCell>
          </TableRow>
        </TableHeader>
        <TableBody>
          {search.rows.map((row) => (
            <TableRow key={row.id}>
              <TableCell>{row.name}</TableCell>
              <TableCell>{row.code}</TableCell>
              <TableCell>{t(`units.${row.baseUnit}`)}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <IngredientPagination
        query={search.query}
        sort={sort}
        page={search.page}
        totalPages={search.totalPages}
      />
    </VStack>
  );
}

function SortableHeader({
  search,
  sortKey,
}: {
  search: IngredientSearchViewModel;
  sortKey: Exclude<SortKey, 'relevance'>;
}) {
  const t = useTranslations('foodpedia.table');
  const isSorted = search.sort.key === sortKey;
  return (
    <TableHeaderCell
      scope="col"
      aria-sort={isSorted ? search.sort.direction : 'none'}
    >
      <Link
        href={toSearchHref({
          q: search.query,
          sort: toNextSort({ current: search.sort, key: sortKey }),
        })}
      >
        {t(sortKey)}
      </Link>
    </TableHeaderCell>
  );
}
