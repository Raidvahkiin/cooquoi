import { backendClient } from '@/services';
import { NextResponse } from 'next/server';

export async function DELETE(
  _request: Request,
  { params }: { params: Promise<{ id: string }> },
) {
  const { id } = await params;
  await backendClient.deleteIngredient(id);
  return new NextResponse(null, { status: 204 });
}
