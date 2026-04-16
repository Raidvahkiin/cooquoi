import { Command } from '@nestjs/cqrs';

export class DeleteIngredientCommand extends Command<void> {
  constructor(public readonly id: string) {
    super();
  }
}
