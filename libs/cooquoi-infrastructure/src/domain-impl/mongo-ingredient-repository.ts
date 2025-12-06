import { Ingredient, IngredientRepository } from "@cooquoi/domain";
import { Injectable } from "@nestjs/common";
import { InjectModel } from "@nestjs/mongoose";
import { Model } from "mongoose";
import { IngredientModel } from "../mongoose/models/ingredient.model";

@Injectable()
export class MongoIngredientRepository implements IngredientRepository {
	constructor(
		@InjectModel(IngredientModel.name)
		private readonly ingredientModel: Model<IngredientModel>,
	) {}

	async save(ingredient: Ingredient): Promise<void> {
		const model = IngredientModel.mapper.fromEntity(ingredient);
		await this.ingredientModel.findByIdAndUpdate(model._id, model, {
			upsert: true,
			new: true,
		});
	}

	async findById(id: string): Promise<Ingredient | null> {
		const model = await this.ingredientModel.findById(id).exec();
		if (!model) {
			return null;
		}
		return IngredientModel.mapper.toEntity(model);
	}

	async findMany(filters: string[]): Promise<Ingredient[]> {
		// TODO: implement proper filtering
		const models = await this.ingredientModel.find().exec();
		return models.map((model) => IngredientModel.mapper.toEntity(model));
	}
}
