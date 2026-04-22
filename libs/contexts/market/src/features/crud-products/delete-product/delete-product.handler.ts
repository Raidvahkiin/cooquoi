import { Inject } from '@nestjs/common';
import { CommandHandler, ICommandHandler } from '@nestjs/cqrs';
import { eq } from 'drizzle-orm';
import { DATABASE_TOKEN, type MarketDatabase } from '../../../config';
import { products } from '../../../domain';
import { DeleteProductCommand } from './delete-product.command';

@CommandHandler(DeleteProductCommand)
export class DeleteProductHandler
  implements ICommandHandler<DeleteProductCommand, void>
{
  constructor(@Inject(DATABASE_TOKEN) private readonly db: MarketDatabase) {}

  async execute(command: DeleteProductCommand): Promise<void> {
    await this.db.delete(products).where(eq(products.id, command.id));
  }
}
