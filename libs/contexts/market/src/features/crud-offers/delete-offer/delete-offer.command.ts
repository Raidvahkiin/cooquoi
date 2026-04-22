import { Command } from '@nestjs/cqrs';

export class DeleteOfferCommand extends Command<void> {
  constructor(public readonly id: string) {
    super();
  }
}
