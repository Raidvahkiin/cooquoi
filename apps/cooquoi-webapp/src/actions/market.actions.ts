'use server';

import { cooquoiClient } from '@/services';
import type { CreateIngredientDto, IngredientDto } from '@/types/ingredient';

export async function searchIngredients(
  filter: string,
  { skip = 0, take = 20 }: { skip?: number; take?: number } = {},
): Promise<{ items: IngredientDto[]; total: number }> {
  const trimmed = filter.trim();
  const result = await cooquoiClient.ingredients.getMany({
    skip,
    take,
    search: trimmed || undefined,
    sortField: 'name',
    sortOrder: 'asc',
  });
  return { items: result.items, total: result.total };
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

export async function deleteIngredient(id: string): Promise<void> {
  await cooquoiClient.ingredients.delete(id);
}

export async function createIngredient(
  dto: CreateIngredientDto,
): Promise<IngredientDto> {
  return cooquoiClient.ingredients.create({
    name: dto.name.trim(),
    description: dto.description,
  });
}
