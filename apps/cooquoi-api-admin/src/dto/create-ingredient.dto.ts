import { ApiProperty } from "@nestjs/swagger";

export class CreateIngredientDto {
	@ApiProperty({
		description: "Name of the ingredient",
		example: "Salt",
		type: String,
	})
	name!: string;

	@ApiProperty({
		description: "Description of the ingredient",
		example: "A common seasoning",
		type: String,
	})
	description!: string;
}
