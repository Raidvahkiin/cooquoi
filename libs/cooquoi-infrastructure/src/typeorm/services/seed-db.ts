import { Ingredient, IngredientAmount, Product } from "@cooquoi/domain";
import { PieceUnit, Unit, WeightUnit } from "@libs/core";
import { Injectable, Logger } from "@nestjs/common";
import { InjectRepository } from "@nestjs/typeorm";
import { Repository } from "typeorm";
import { DbSeedable } from "../../interfaces";
import { IngredientMapper, ProductMapper } from "../mappers";
import { IngredientModel, ProductModel } from "../models";

const ingredientsData = [
	{
		name: "Tomato",
		description: "A red juicy vegetable",
	},
	{
		name: "Lettuce",
		description: "A leafy green vegetable",
	},
	{
		name: "Cheese",
		description: "Dairy product made from milk",
	},
	{
		name: "Chocolate",
		description: "Sweet cocoa-based confection",
	},
	{
		name: "Hazelnut",
		description: "Edible nut from hazel trees",
	},
	{
		name: "Milk",
		description: "Dairy beverage",
	},
	{
		name: "Flour",
		description: "Ground wheat powder",
	},
	{
		name: "Sugar",
		description: "Sweet crystalline carbohydrate",
	},
	{
		name: "Butter",
		description: "Dairy product made from milk fat",
	},
	{
		name: "Egg",
		description: "Whole chicken egg",
	},
	{
		name: "Egg Yolk",
		description: "Yellow part of an egg",
	},
	{
		name: "Egg White",
		description: "Clear part of an egg",
	},
	{
		name: "Egg Shell",
		description: "Outer shell of an egg",
	},
];

type ProductSeedData = {
	name: string;
	aliases: string[];
	ingredients: Array<{
		ingredientName: string;
		quantity: number;
		unit: Unit;
	}>;
	customAttributes: Record<string, string | number | boolean>;
};

const productsData: ProductSeedData[] = [
	{
		name: "Barres Chocolatées Lait Noisette KINDER BUENO",
		aliases: ["KIND BUE Barre Choco lot 2", "KINDER BUENO", "Bueno Chocolate"],
		ingredients: [
			{ ingredientName: "Chocolate", quantity: 30, unit: WeightUnit.GRAM },
			{ ingredientName: "Hazelnut", quantity: 10, unit: WeightUnit.GRAM },
			{ ingredientName: "Milk", quantity: 3, unit: WeightUnit.GRAM },
		],
		customAttributes: {
			brand: "KINDER",
			language: "FR",
			packageSize: 2,
			flavor: "Milk Hazelnut",
		},
	},
	{
		name: "Egg Pack Premium - 6 Eggs",
		aliases: ["EGG PACK 6", "Premium Eggs"],
		ingredients: [
			{ ingredientName: "Egg", quantity: 6, unit: PieceUnit.PIECE },
			{ ingredientName: "Egg Yolk", quantity: 6, unit: PieceUnit.PIECE },
			{ ingredientName: "Egg White", quantity: 6, unit: PieceUnit.PIECE },
			{ ingredientName: "Egg Shell", quantity: 6, unit: PieceUnit.PIECE },
		],
		customAttributes: {
			brand: "Farm Fresh",
			language: "EN",
			packageSize: 6,
			quality: "Premium",
		},
	},
];

@Injectable()
export class SeedDb implements DbSeedable {
	private readonly logger: Logger = new Logger(SeedDb.name);

	constructor(
		@InjectRepository(IngredientModel)
		private readonly _ingredients: Repository<IngredientModel>,

		@InjectRepository(ProductModel)
		private readonly _products: Repository<ProductModel>,
	) {}

	async seedInitialData() {
		this.logger.debug("Sedding initial data...");
		console.debug("Sedding initial data...");

		const ingredients = ingredientsData.map((data) => new Ingredient(data));

		const products = productsData.map((data) => {
			const ingredientsMap = new Map();

			for (const ingData of data.ingredients) {
				const ingredient = ingredients.find(
					(i) => i.name === ingData.ingredientName,
				);
				if (!ingredient) {
					throw new Error(`Ingredient ${ingData.ingredientName} not found`);
				}
				ingredientsMap.set(
					ingredient,
					IngredientAmount.create(ingData.quantity, ingData.unit),
				);
			}

			return new Product({
				name: data.name,
				ingredients: ingredientsMap,
				aliases: data.aliases,
				customAttributes: data.customAttributes,
			});
		});

		for (const ingredient of ingredients) {
			const existing = await this._ingredients.findOne({
				where: { name: ingredient.name },
			});

			if (!existing) {
				const model = IngredientMapper.fromEntity(ingredient).toModel();
				await this._ingredients.save(model);
				this.logger.debug(`Seeded ingredient: ${ingredient.name}`);
			} else {
				this.logger.debug(`Skipped existing ingredient: ${ingredient.name}`);
			}
		}

		for (const product of products) {
			const existing = await this._products.findOne({
				where: { name: product.name },
			});

			if (!existing) {
				const productModel = ProductMapper.fromEntity(product).toModel();

				// Load all ingredient models for this product
				const ingredientModels: IngredientModel[] = [];
				for (const [ingredient] of product.ingredients.entries()) {
					const ingredientModel = await this._ingredients.findOne({
						where: { name: ingredient.name },
					});
					if (ingredientModel) {
						ingredientModels.push(ingredientModel);
					}
				}

				productModel.ingredients = ingredientModels;

				await this._products.save(productModel);
				this.logger.debug(`Seeded product: ${product.name}`);
			} else {
				this.logger.debug(`Skipped existing product: ${product.name}`);
			}
		}

		this.logger.debug("Seeding initial data... Done");
	}
}
