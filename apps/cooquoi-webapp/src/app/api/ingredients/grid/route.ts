import { backendClient } from "@/services";
import { NextResponse } from "next/server";

type AgGridSortDirection = "asc" | "desc";

type AgGridSortModelItem = {
	colId: string;
	sort: AgGridSortDirection;
};

type AgGridServerSideRequest = {
	startRow: number;
	endRow: number;
	sortModel?: AgGridSortModelItem[];
	filterModel?: Record<string, unknown>;
};

export async function POST(request: Request) {
	const body = (await request.json()) as Partial<AgGridServerSideRequest>;

	const startRow = Number(body.startRow);
	const endRow = Number(body.endRow);

	if (
		!Number.isFinite(startRow) ||
		!Number.isFinite(endRow) ||
		endRow < startRow
	) {
		return NextResponse.json(
			{ message: "Invalid startRow/endRow" },
			{ status: 400 },
		);
	}

	const result = await backendClient.getIngredientsGrid({
		startRow,
		endRow,
		sortModel: body.sortModel,
		filterModel: body.filterModel,
	});

	return NextResponse.json(result);
}
