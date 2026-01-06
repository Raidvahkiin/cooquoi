import { ProductRepository } from "@cooquoi/domain";
import { CommandHandler, ICommandHandler } from "@nestjs/cqrs";
import { DeleteIngredientCommand } from "./delete-ingredient.command";

@CommandHandler(DeleteIngredientCommand)
export class DeleteIngredientCommandHandler
	implements ICommandHandler<DeleteIngredientCommand>
{
	constructor(private readonly ingredientRepository: ProductRepository) {}

	async execute(command: DeleteIngredientCommand): Promise<void> {
		await this.ingredientRepository.deleteById(command.id);
	}
}
