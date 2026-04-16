import { Command } from '@nestjs/cqrs';
import { EnableLogging } from '@utils/nestjs/cqrs';
import { Ingredient } from '../../../domain';

@EnableLogging()
export class CreateIngredientCommand extends Command<Ingredient> {
  constructor(
    public readonly name: string,
    public readonly description?: string,
  ) {
    super();
  }
}
