import { ApiProperty } from "@nestjs/swagger";
import { Ingredient } from "@cooquoi/presentation";

export class IngredientResponseDto {
	@ApiProperty()
	id: string;

	@ApiProperty()
	name: string;

	@ApiProperty({ nullable: true, type: String })
	description: string | null;

	@ApiProperty()
	createdAt: Date;

	@ApiProperty()
	updatedAt: Date;

	constructor(ingredient: Ingredient) {
		this.id = ingredient.id;
		this.name = ingredient.name;
		this.description = ingredient.description;
		this.createdAt = ingredient.createdAt;
		this.updatedAt = ingredient.updatedAt;
	}
}
