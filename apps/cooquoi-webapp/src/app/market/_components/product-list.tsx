'use client';

import { useState } from 'react';
import { ProductCard } from './product-card';

type Offer = {
  id: string;
  vendor: string;
  updatedAt: string;
  price: { amount: number; currency: string };
};

type Product = {
  id: string;
  name: string;
  description: string | null;
  ingredients: string[];
  offers: Offer[];
};

export function ProductList({ products }: { products: Product[] }) {
  const [openId, setOpenId] = useState<string | null>(null);

  return (
    <ul className="flex flex-col gap-3">
      {products.map((product) => (
        <li key={product.id}>
          <ProductCard
            product={product}
            open={openId === product.id}
            onToggle={() =>
              setOpenId((prev) => (prev === product.id ? null : product.id))
            }
          />
        </li>
      ))}
    </ul>
  );
}
