import {
  CreateOrUpdateOfferCommand,
  type CreateOrUpdateOfferCommandPayload,
  DeleteOfferCommand,
  type Offer,
  Price,
} from '@cooquoi/market';
import { Body, Controller, Delete, Param, Post } from '@nestjs/common';
import { CommandBus } from '@nestjs/cqrs';
import type {
  CreateOrUpdateOfferBody,
  OffersEndpoints,
} from '@utils/api-contracts/cooquoi';

@Controller('offers')
export class OffersController implements OffersEndpoints {
  constructor(private readonly commandBus: CommandBus) {}

  @Post()
  createOrUpdate(@Body() body: CreateOrUpdateOfferBody): Promise<Offer> {
    const payload: CreateOrUpdateOfferCommandPayload = {
      productId: body.productId,
      vendor: body.vendor,
      price: Price.create(body.price),
    };
    return this.commandBus.execute<CreateOrUpdateOfferCommand, Offer>(
      new CreateOrUpdateOfferCommand(payload),
    );
  }

  @Delete(':id')
  async delete(@Param('id') id: string): Promise<void> {
    await this.commandBus.execute(new DeleteOfferCommand(id));
  }
}
