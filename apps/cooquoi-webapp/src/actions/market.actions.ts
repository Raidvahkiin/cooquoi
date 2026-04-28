'use server';

import { cooquoiClient } from '@/services';
import type { IngredientDto } from '@/types/ingredient';

export async function searchIngredients(
  filter: string,
  { take = 20 }: { take?: number } = {},
): Promise<IngredientDto[]> {
  const trimmed = filter.trim();
  const result = await cooquoiClient.ingredients.getMany({
    skip: 0,
    take,
    search: trimmed || undefined,
    sortField: 'name',
    sortOrder: 'asc',
  });
  return result.items;
}

export async function checkProductNameExists(name: string): Promise<boolean> {
  const trimmed = name.trim();
  if (!trimmed) return false;
  const { items } = await cooquoiClient.products.getMany({
    search: trimmed,
    take: 10,
  });
  return items.some((p) => p.name.toLowerCase() === trimmed.toLowerCase());
}

export async function createProduct(payload: {
  name: string;
  description?: string;
  ingredients: string[];
}): Promise<{ id: string }> {
  return cooquoiClient.products.create(payload);
}

export async function createOffers(
  offers: {
    productId: string;
    vendor: string;
    price: { amount: string; currency: string };
  }[],
): Promise<void> {
  await Promise.all(
    offers.map((o) =>
      cooquoiClient.offers.createOrUpdate({
        productId: o.productId,
        vendor: o.vendor.trim(),
        price: { amount: o.price.amount, currency: o.price.currency },
      }),
    ),
  );
}
