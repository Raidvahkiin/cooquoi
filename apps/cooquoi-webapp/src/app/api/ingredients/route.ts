import { backendClient } from "@/services";
import type { CreateIngredientDto } from "@/types/ingredient";
import { NextResponse } from "next/server";

export async function GET() {
	const ingredients = await backendClient.getIngredients();
	return NextResponse.json(ingredients);
}

export async function POST(request: Request) {
	const dto = (await request.json()) as CreateIngredientDto;

	if (!dto?.name?.trim()) {
		return NextResponse.json(
			{ message: "name is required" },
			{ status: 400 },
		);
	}

	await backendClient.createIngredient({
		name: dto.name.trim(),
		description: dto.description ?? "",
	});

	return new NextResponse(null, { status: 201 });
}
