import type { FoodpediaIngredientSummary } from '@cooquoi/api-client';

type Locale = 'en' | 'fr' | 'vi';

const names: Record<Locale, [string, string]> = {
  en: ['Salmon', 'Rice'],
  fr: ['Saumon', 'Riz'],
  vi: ['Cá hồi', 'Gạo'],
};

/** Two ingredients named in the requested locale, as the API returns them. */
export function ingredientSummaries(
  locale: Locale,
): FoodpediaIngredientSummary[] {
  const [salmon, rice] = names[locale];
  return [
    {
      id: '0198f0a0-0000-7000-8000-0000000000a1',
      code: 'salmon',
      name: salmon,
      baseUnit: 'g',
      categoryTags: ['category:fish'],
    },
    {
      id: '0198f0a0-0000-7000-8000-0000000000a2',
      code: 'rice',
      name: rice,
      baseUnit: 'g',
      categoryTags: ['category:grain'],
    },
  ];
}
