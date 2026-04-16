import { CreateProductCommand, type Product } from '@cooquoi/market';
import { Body, Controller, Post } from '@nestjs/common';
import { CommandBus } from '@nestjs/cqrs';
import {
  CreateProductBody,
  ProductsEndpoints,
} from '@utils/api-contracts/cooquoi';

@Controller('products')
export class ProductsController implements ProductsEndpoints {
  constructor(private readonly commandBus: CommandBus) {}

  @Post()
  create(@Body() body: CreateProductBody): Promise<Product> {
    return this.commandBus.execute<CreateProductCommand, Product>(
      new CreateProductCommand(body),
    );
  }
}
