import { Prop, Schema, SchemaFactory } from "@nestjs/mongoose";
import { HydratedDocument } from "mongoose";
import { Ingredient } from "@cooquoi/domain";

export type IngredientDocument = HydratedDocument<IngredientModel>;

export class IngredientMapper {
	fromEntity(entity: Ingredient): IngredientModel {
		const model = new IngredientModel();
		model._id = entity.id;
		model.name = entity.name;
		model.description = entity.description;
		model.createdAt = entity.createdAt;
		model.updatedAt = entity.updatedAt;
		return model;
	}

	toEntity(model: IngredientModel): Ingredient {
		return new Ingredient({
			id: model._id,
			name: model.name,
			description: model.description,
			createdAt: model.createdAt,
			updatedAt: model.updatedAt,
		});
	}
}

@Schema({ collection: "ingredients", timestamps: false })
export class IngredientModel {
	@Prop({ required: true, type: String })
	_id!: string;

	@Prop({ required: true, type: String })
	name!: string;

	@Prop({ type: String, default: null })
	description!: string | null;

	@Prop({ required: true, type: Date })
	createdAt!: Date;

	@Prop({ required: true, type: Date })
	updatedAt!: Date;

	static mapper = new IngredientMapper();
}

export const IngredientSchema = SchemaFactory.createForClass(IngredientModel);
