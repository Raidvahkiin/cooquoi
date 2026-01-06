import {
	Ingredient,
	IngredientAmount,
	IngredientRepository,
	Product,
	ProductRepository,
} from "@cooquoi/domain";
import { InvalidOperationError } from "@cooquoi/domain";
import { DatetimeProvider } from "@libs/core";
import { CommandHandler, ICommandHandler } from "@nestjs/cqrs";
import { CreateProductCommand } from "./create-product.command";

@CommandHandler(CreateProductCommand)
export class CreateProductCommandHandler
	implements ICommandHandler<CreateProductCommand>
{
	constructor(
		private readonly productRepository: ProductRepository,
		private readonly ingredientRepository: IngredientRepository,
		private readonly datetimeProvider: DatetimeProvider,
	) {}
	async execute(command: CreateProductCommand): Promise<void> {
		// Validate and fetch all ingredients
		const ingredientsMap = new Map<Ingredient, IngredientAmount>();

		for (const ingData of command.ingredients) {
			const ingredient = await this.ingredientRepository.findById(
				ingData.ingredientId,
			);
			if (!ingredient) {
				throw new InvalidOperationError(
					`Ingredient with ID ${ingData.ingredientId} not found`,
				);
			}
			ingredientsMap.set(ingredient, ingData.quantity);
		}

		// Create product with multiple ingredients
		const product = new Product(
			// state
			{
				name: command.name,
				aliases: command.aliases,
				ingredients: ingredientsMap,
				customAttributes: command.customAttributes,
			},

			// options
			{
				datetimeProvider: this.datetimeProvider,
			},
		);

		await this.productRepository.save(product);
	}
}
