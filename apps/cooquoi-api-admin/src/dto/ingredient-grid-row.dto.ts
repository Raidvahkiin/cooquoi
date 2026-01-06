import { IngredientModel } from "@cooquoi/presentation";
import { ApiProperty } from "@nestjs/swagger";

export class IngredientGridRowDto {
	@ApiProperty()
	id!: string;

	@ApiProperty()
	name!: string;

	@ApiProperty({ nullable: true, type: String })
	description!: string | null;

	@ApiProperty()
	createdAt!: Date;

	@ApiProperty()
	updatedAt!: Date;

	static fromModel(model: IngredientModel): IngredientGridRowDto {
		return {
			id: model.id,
			name: model.name,
			description: model.description ?? null,
			createdAt: model.createdAt,
			updatedAt: model.updatedAt,
		};
	}
}
