import { Command } from '@nestjs/cqrs';
import { EnableLogging } from '@utils/nestjs/cqrs';
import { Offer, Price } from '../../../domain';

export interface CreateOrUpdateOfferCommandPayload {
  productId: string;
  vendor: string;
  price: Price;
}

@EnableLogging()
export class CreateOrUpdateOfferCommand extends Command<Offer> {
  public readonly productId: string;
  public readonly vendor: string;
  public readonly price: Price;

  constructor(payload: CreateOrUpdateOfferCommandPayload) {
    super();
    this.productId = payload.productId;
    this.vendor = payload.vendor;
    this.price = payload.price;
  }
}
