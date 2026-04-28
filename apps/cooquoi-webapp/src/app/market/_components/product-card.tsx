'use client';

import { ChevronDownIcon, cn } from '@utils/react/ui';
import Link from 'next/link';

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

function formatPrice(offer: Offer) {
  return new Intl.NumberFormat('fr-FR', {
    style: 'currency',
    currency: offer.price.currency,
  }).format(offer.price.amount);
}

function lowestOffer(offers: Offer[]) {
  return offers.reduce((best, o) =>
    o.price.amount < best.price.amount ? o : best,
  );
}

export function ProductCard({
  product,
  open,
  onToggle,
}: {
  product: Product;
  open: boolean;
  onToggle: () => void;
}) {
  const best = lowestOffer(product.offers);

  return (
    <div className="overflow-hidden rounded-xl bg-white shadow-sm ring-1 ring-black/5">
      {/* Main row */}
      <button
        type="button"
        onClick={() => onToggle()}
        className="flex w-full items-center gap-4 p-4 text-left"
      >
        {/* Thumbnail — links to detail page */}
        <Link
          href={`/market/${product.id}`}
          onClick={(e) => e.stopPropagation()}
          className="flex h-16 w-16 shrink-0 items-center justify-center rounded-lg bg-neutral-100 text-2xl font-bold text-neutral-400 hover:bg-neutral-200 transition-colors"
        >
          {product.name[0]}
        </Link>

        {/* Info */}
        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-semibold text-neutral-800">
            {product.name}
          </p>
          <p className="mt-0.5 text-base font-bold text-emerald-600">
            {formatPrice(best)}
          </p>
          <p className="text-xs text-neutral-400">
            {product.offers.length === 1
              ? '1 vendor'
              : `${product.offers.length} vendors`}
          </p>
        </div>

        {/* Expand chevron */}
        <span
          className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full"
          aria-hidden="true"
        >
          <ChevronDownIcon
            className={cn(
              'text-neutral-400 transition-transform duration-200',
              open && 'rotate-180',
            )}
          />
        </span>
      </button>

      {/* Expanded content */}
      {open && (
        <div className="border-t border-neutral-100 px-4 pb-4 pt-3">
          {product.description && (
            <p className="mb-2 text-sm text-neutral-500">
              {product.description}
            </p>
          )}
          <p className="mb-3 text-xs text-neutral-400">
            <span className="font-medium text-neutral-500">Ingredients: </span>
            {product.ingredients.join(', ')}
          </p>

          <div className="flex flex-col gap-2">
            {product.offers
              .slice()
              .sort((a, b) => a.price.amount - b.price.amount)
              .map((offer) => (
                <div
                  key={offer.id}
                  className={cn(
                    'flex items-center justify-between rounded-lg px-3 py-2 text-sm',
                    offer.id === best.id
                      ? 'bg-emerald-50 ring-1 ring-emerald-200'
                      : 'bg-neutral-50',
                  )}
                >
                  <span className="font-medium text-neutral-700">
                    {offer.vendor}
                  </span>
                  <span
                    className={cn(
                      'font-semibold',
                      offer.id === best.id
                        ? 'text-emerald-600'
                        : 'text-neutral-600',
                    )}
                  >
                    {formatPrice(offer)}
                  </span>
                </div>
              ))}
          </div>
        </div>
      )}
    </div>
  );
}
