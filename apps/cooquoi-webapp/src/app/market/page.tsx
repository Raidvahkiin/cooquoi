import { cooquoiClient } from '@/services/backend.http-client';
import { Suspense } from 'react';
import { SearchInput } from './_components/search-input';
import { ProductList } from './_components/product-list';

type PageProps = {
  searchParams: Promise<{ search?: string }>;
};

export default async function MarketPage({ searchParams }: PageProps) {
  const { search } = await searchParams;
  const { items } = await cooquoiClient.products.getMany({
    take: 50,
    maxOffers: 5,
    ...(search ? { search } : {}),
  });

  const products = items.map((p) => ({
    id: p.id,
    name: p.name,
    description: p.description,
    ingredients: p.ingredients,
    offers: p.offers.map((o) => ({
      id: o.id,
      vendor: o.vendor,
      updatedAt: o.updatedAt,
      price: {
        amount: Number.parseFloat(o.priceAmount),
        currency: o.priceCurrency,
      },
    })),
  }));

  return (
    <div className="max-w-7xl mx-auto flex flex-col gap-4 min-h-screen p-4">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-neutral-800">Market</h1>
      </div>
      <Suspense>
        <SearchInput />
      </Suspense>
      <ProductList products={products} />
    </div>
  );
}
