import {
  IngredientSearch,
  parseIngredientSearchParams,
  searchIngredientsPage,
} from '@/features/foodpedia';

type FoodpediaPageProps = {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
};

export default async function FoodpediaPage({
  searchParams,
}: FoodpediaPageProps) {
  const params = parseIngredientSearchParams(await searchParams);
  const result = await searchIngredientsPage(params);
  return <IngredientSearch result={result} sort={params.sort} />;
}
