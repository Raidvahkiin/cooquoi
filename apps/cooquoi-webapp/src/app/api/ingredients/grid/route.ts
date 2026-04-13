import { backendClient } from '@/services';
import { NextResponse } from 'next/server';

type AgGridFilterModel = Record<string, { filter?: string }>;

type AgGridSortModelItem = {
  colId: string;
  sort: 'asc' | 'desc';
};

type AgGridGridRequest = {
  startRow: number;
  endRow: number;
  filterModel?: AgGridFilterModel;
  sortModel?: AgGridSortModelItem[];
};

export async function POST(request: Request) {
  const body = (await request.json()) as Partial<AgGridGridRequest>;

  const startRow = Number(body.startRow);
  const endRow = Number(body.endRow);

  if (
    !Number.isFinite(startRow) ||
    !Number.isFinite(endRow) ||
    endRow < startRow
  ) {
    return NextResponse.json(
      { message: 'Invalid startRow/endRow' },
      { status: 400 },
    );
  }

  const search = body.filterModel?.name?.filter;
  const take = endRow - startRow;
  const firstSort = body.sortModel?.[0];

  const result = await backendClient.filterIngredients({
    skip: startRow,
    take,
    search,
    sortField: firstSort?.colId,
    sortOrder: firstSort?.sort,
  });

  return NextResponse.json({
    rows: result.items,
    lastRow: result.total,
  });
}
