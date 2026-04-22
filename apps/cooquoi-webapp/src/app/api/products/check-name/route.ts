import { cooquoiClient } from '@/services';
import { NextRequest, NextResponse } from 'next/server';

export async function GET(request: NextRequest) {
  const name = request.nextUrl.searchParams.get('name')?.trim();

  if (!name) {
    return NextResponse.json({ exists: false });
  }

  const { items } = await cooquoiClient.products.getMany({
    search: name,
    take: 10,
  });
  const exists = items.some((p) => p.name.toLowerCase() === name.toLowerCase());

  return NextResponse.json({ exists });
}
