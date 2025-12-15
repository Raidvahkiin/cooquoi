import { CommandHandler, ICommandHandler } from "@nestjs/cqrs";
import { IngredientRepository } from "@cooquoi/domain";
import { DeleteIngredientCommand } from "./delete-ingredient.command";

@CommandHandler(DeleteIngredientCommand)
export class DeleteIngredientCommandHandler
	implements ICommandHandler<DeleteIngredientCommand>
{
	constructor(private readonly ingredientRepository: IngredientRepository) {}

	async execute(command: DeleteIngredientCommand): Promise<void> {
		await this.ingredientRepository.deleteById(command.id);
	}
}
