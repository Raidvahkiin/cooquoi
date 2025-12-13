import { Ingredient } from "@cooquoi/domain";
import { Injectable, Logger } from "@nestjs/common";
import { Model } from "mongoose";
import { InjectModel } from "@nestjs/mongoose";
import { IngredientModel } from "../models";

@Injectable()
export class SeedDb {
	private readonly logger: Logger = new Logger(SeedDb.name);

	constructor(
		@InjectModel(IngredientModel.name)
		private readonly _ingredients: Model<IngredientModel>,
	) {}

	async seedInitialData() {
		this.logger.debug("Sedding initial data...");
		await this._ingredients.deleteMany({});
		const ingredients = [
			new Ingredient({ name: "Tomato", description: "A red juicy vegetable" }),
			new Ingredient({
				name: "Lettuce",
				description: "A leafy green vegetable",
			}),
			new Ingredient({
				name: "Cheese",
				description: "Dairy product made from milk",
			}),
		].map((ingredient) => IngredientModel.mapper.fromEntity(ingredient));
		await this._ingredients.insertMany(ingredients);
		this.logger.debug("Seeding initial data... Done");
	}
}
