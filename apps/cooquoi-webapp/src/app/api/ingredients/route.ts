import { backendClient } from '@/services';
import type { CreateIngredientDto } from '@/types/ingredient';
import { NextRequest, NextResponse } from 'next/server';

export async function GET(request: NextRequest) {
  const { searchParams } = request.nextUrl;
  const skip = Number(searchParams.get('skip') ?? 0);
  const take = Number(searchParams.get('take') ?? 20);
  const search = searchParams.get('search') ?? undefined;

  const result = await backendClient.filterIngredients({ skip, take, search });
  return NextResponse.json(result);
}

export async function POST(request: Request) {
  const dto = (await request.json()) as CreateIngredientDto;

  if (!dto?.name?.trim()) {
    return NextResponse.json({ message: 'name is required' }, { status: 400 });
  }

  const created = await backendClient.createIngredient({
    name: dto.name.trim(),
    description: dto.description,
  });

  return NextResponse.json(created, { status: 201 });
}
