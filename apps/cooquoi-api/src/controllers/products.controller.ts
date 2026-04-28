import {
  CreateProductCommand,
  DeleteProductCommand,
  FilterProductsDto,
  FilterProductsQuery,
  type FilterProductsResult,
  type Product,
  type ProductWithOffers,
} from '@cooquoi/market';
import {
  Body,
  Controller,
  Delete,
  Get,
  Param,
  Post,
  Query,
} from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import {
  type CreateProductBody,
  type FilterResult,
  type ProductsEndpoints,
} from '@utils/api-contracts/cooquoi';

@Controller('products')
export class ProductsController implements ProductsEndpoints {
  constructor(
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus,
  ) {}

  @Post()
  create(@Body() body: CreateProductBody): Promise<Product> {
    return this.commandBus.execute<CreateProductCommand, Product>(
      new CreateProductCommand(body),
    );
  }

  @Get()
  getMany(
    @Query() query: FilterProductsDto,
  ): Promise<FilterResult<ProductWithOffers>> {
    return this.queryBus.execute<FilterProductsQuery, FilterProductsResult>(
      new FilterProductsQuery({
        skip: query.skip ?? 0,
        take: query.take ?? 20,
        search: query.search,
        maxOffers: query.maxOffers,
        sort: query.sortField
          ? { field: query.sortField, order: query.sortOrder ?? 'asc' }
          : undefined,
      }),
    );
  }

  @Delete(':id')
  async delete(@Param('id') id: string): Promise<void> {
    await this.commandBus.execute(new DeleteProductCommand(id));
  }
}
