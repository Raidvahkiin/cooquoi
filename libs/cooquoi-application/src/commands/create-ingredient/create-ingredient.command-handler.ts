import { Ingredient, IngredientRepository } from "@cooquoi/domain";
import { DatetimeProvider } from "@libs/core";
import { CommandHandler, ICommandHandler } from "@nestjs/cqrs";
import { CreateIngredientCommand } from "./create-ingredient.command";

@CommandHandler(CreateIngredientCommand)
export class CreateIngredientCommandHandler
	implements ICommandHandler<CreateIngredientCommand>
{
	constructor(
		private readonly ingredientRepository: IngredientRepository,
		private readonly datetimeProvider: DatetimeProvider,
	) {}
	async execute(command: CreateIngredientCommand): Promise<void> {
		// Implementation for creating an ingredient goes here
		const ingredient = new Ingredient(
			// state
			{
				name: command.name,
				description: command.description,
			},

			// options
			{
				datetimeProvider: this.datetimeProvider,
			},
		);

		await this.ingredientRepository.save(ingredient);
	}
}
