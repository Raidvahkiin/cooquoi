import { cooquoiClient } from '@/services';
import { NextRequest, NextResponse } from 'next/server';

export async function POST(request: NextRequest) {
  const body = await request.json();

  if (!body?.name?.trim()) {
    return NextResponse.json({ message: 'name is required' }, { status: 400 });
  }

  const product = await cooquoiClient.products.create({
    name: body.name.trim(),
    description: body.description,
    ingredients: body.ingredients ?? [],
  });

  return NextResponse.json(product, { status: 201 });
}
