import { Ingredient, IngredientRepository } from "@cooquoi/domain";
import { EntityFilter } from "@libs/core";
import { Injectable } from "@nestjs/common";
import { InjectModel } from "@nestjs/mongoose";
import { Model } from "mongoose";
import { IngredientFilterProcessor, IngredientModel } from "../mongoose";

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

	async deleteById(id: string): Promise<void> {
		await this.ingredientModel.findByIdAndDelete(id).exec();
	}

	async findById(id: string): Promise<Ingredient | null> {
		const model = await this.ingredientModel.findById(id).exec();
		if (!model) {
			return null;
		}
		return IngredientModel.mapper.toEntity(model);
	}

	async findMany(filters: EntityFilter<Ingredient>[]): Promise<Ingredient[]> {
		const query = new IngredientFilterProcessor().process(filters);
		const models = await this.ingredientModel.find(query).exec();
		return models.map((model) => IngredientModel.mapper.toEntity(model));
	}
}
