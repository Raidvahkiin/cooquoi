import { searchIngredients } from '@/actions/market.actions';
import { Suspense } from 'react';
import { IngredientList } from './_components/ingredient-list';
import { IngredientsSearchInput } from './_components/ingredients-search-input';
import { Pagination } from './_components/pagination';
import { Stack } from '@utils/react/ui';
import { Header } from './_components/header';

const PAGE_SIZE = 5;

type PageProps = {
  searchParams: Promise<{ search?: string; page?: string }>;
};

export default async function IngredientsPage({ searchParams }: PageProps) {
  const { search, page: pageParam } = await searchParams;
  const page = Math.max(1, Number(pageParam ?? 1));
  const skip = (page - 1) * PAGE_SIZE;

  const { items, total } = await searchIngredients(search ?? '', {
    skip,
    take: PAGE_SIZE,
  });

  return (
    <Stack className="mx-auto w-full max-w-7xl px-4 py-6" gap="lg">
      <Header />
      <Suspense>
        <IngredientsSearchInput />
      </Suspense>
      <IngredientList ingredients={items} />
      {Math.ceil(total / PAGE_SIZE) > 1 ? (
        <Suspense>
          <Pagination
            page={page}
            totalPages={Math.ceil(total / PAGE_SIZE)}
            total={total}
          />
        </Suspense>
      ) : null}
    </Stack>
  );
}
