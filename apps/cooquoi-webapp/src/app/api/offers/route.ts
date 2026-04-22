import { cooquoiClient } from '@/services';
import { NextRequest, NextResponse } from 'next/server';

type OfferInput = {
  productId: string;
  vendor: string;
  price: { amount: string; currency: string };
};

function isValidOffer(o: unknown): o is OfferInput {
  if (!o || typeof o !== 'object') return false;
  const offer = o as Record<string, unknown>;
  return (
    typeof offer.productId === 'string' &&
    typeof offer.vendor === 'string' &&
    typeof offer.price === 'object' &&
    offer.price !== null &&
    typeof (offer.price as Record<string, unknown>).amount === 'string' &&
    typeof (offer.price as Record<string, unknown>).currency === 'string'
  );
}

export async function POST(request: NextRequest) {
  const body = await request.json();

  // Accept either a single offer or an array
  const items: unknown[] = Array.isArray(body) ? body : [body];

  if (items.length === 0 || !items.every(isValidOffer)) {
    return NextResponse.json(
      { message: 'Each offer requires productId, vendor and price' },
      { status: 400 },
    );
  }

  const offers = await Promise.all(
    items.map((o) =>
      cooquoiClient.offers.createOrUpdate({
        productId: o.productId,
        vendor: o.vendor.trim(),
        price: { amount: o.price.amount, currency: o.price.currency },
      }),
    ),
  );

  return NextResponse.json(offers.length === 1 ? offers[0] : offers, {
    status: 201,
  });
}
