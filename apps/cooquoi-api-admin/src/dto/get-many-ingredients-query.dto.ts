import { ApiPropertyOptional } from "@nestjs/swagger";

export class GetManyIngredientsQueryDto {
	@ApiPropertyOptional({
		description: "Filter by ingredient id (exact match)",
		example: "b6ff9e23-4a5f-4f41-ae2f-1a59dca6b5e2",
		type: String,
	})
	id?: string;

	@ApiPropertyOptional({
		description:
			"Filter by ingredient ids (in). Comma-separated list, e.g. idIn=a,b,c",
		example:
			"b6ff9e23-4a5f-4f41-ae2f-1a59dca6b5e2,2f6fdc1a-4b6a-4f41-ae2f-1a59dca6b5e3",
		type: String,
	})
	idIn?: string;

	@ApiPropertyOptional({
		description: "Filter by ingredient name (exact match)",
		example: "Salt",
		type: String,
	})
	name?: string;

	@ApiPropertyOptional({
		description:
			"Filter by ingredient names (in). Comma-separated list, e.g. nameIn=Salt,Pepper",
		example: "Salt,Pepper",
		type: String,
	})
	nameIn?: string;

	@ApiPropertyOptional({
		description: "Filter by ingredient description (exact match)",
		example: "A common seasoning",
		type: String,
	})
	description?: string;

	@ApiPropertyOptional({
		description:
			"Filter by ingredient descriptions (in). Comma-separated list, e.g. descriptionIn=a,b",
		example: "A common seasoning,Spicy",
		type: String,
	})
	descriptionIn?: string;
}
