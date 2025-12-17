import { backendClient } from "@/services";
import { NextResponse } from "next/server";

export async function DELETE(
	_request: Request,
	{ params }: { params: Promise<{ id: string }> },
) {
	const { id } = await params;
	if (!id) {
		return NextResponse.json({ message: "id is required" }, { status: 400 });
	}

	await backendClient.deleteIngredient(id);
	return new NextResponse(null, { status: 204 });
}
