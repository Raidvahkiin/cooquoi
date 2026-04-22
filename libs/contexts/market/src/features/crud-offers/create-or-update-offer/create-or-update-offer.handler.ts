import { Inject } from '@nestjs/common';
import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { DATABASE_TOKEN, type MarketDatabase } from '../../../config';
import { Offer, offers } from '../../../domain';
import { CreateOrUpdateOfferCommand } from './create-or-update-offer.command';

@CommandHandler(CreateOrUpdateOfferCommand)
export class CreateOrUpdateOfferHandler
  implements ICommandHandler<CreateOrUpdateOfferCommand, Offer>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: MarketDatabase) {}

  async execute(command: CreateOrUpdateOfferCommand): Promise<Offer> {
    const { productId, vendor, price } = command;

    const [row] = await this.db
      .insert(offers)
      .values({
        productId,
        vendor,
        priceAmount: price.amount,
        priceCurrency: price.currency,
      })
      .onConflictDoUpdate({
        target: [offers.productId, offers.vendor],
        set: {
          priceAmount: price.amount,
          priceCurrency: price.currency,
        },
      })
      .returning();

    return row;
  }
}
