import { ApiProperty } from "@nestjs/swagger";

type IngredientGridRowModel = {
	_id: string;
	name: string;
	description: string | null;
	createdAt: Date;
	updatedAt: Date;
};

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

	static fromModel(model: IngredientGridRowModel): IngredientGridRowDto {
		return {
			id: model._id,
			name: model.name,
			description: model.description,
			createdAt: model.createdAt,
			updatedAt: model.updatedAt,
		};
	}
}
