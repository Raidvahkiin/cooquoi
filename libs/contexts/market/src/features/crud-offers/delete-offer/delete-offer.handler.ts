import { Inject } from '@nestjs/common';
import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { eq } from 'drizzle-orm';
import { DATABASE_TOKEN, type MarketDatabase } from '../../../config';
import { offers } from '../../../domain';
import { DeleteOfferCommand } from './delete-offer.command';

@CommandHandler(DeleteOfferCommand)
export class DeleteOfferHandler
  implements ICommandHandler<DeleteOfferCommand, void>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: MarketDatabase) {}

  async execute(command: DeleteOfferCommand): Promise<void> {
    await this.db.delete(offers).where(eq(offers.id, command.id));
  }
}
