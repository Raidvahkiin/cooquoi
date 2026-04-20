import {
  CreateProductCommand,
  type CreateProductCommandPayload,
  FilterProductsDto,
  FilterProductsQuery,
  type FilterProductsResult,
  type Product,
  type ProductWithOffers,
} from '@cooquoi/market';
import { Body, Controller, Get, Post, Query } from '@nestjs/common';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import {
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
  create(@Body() body: CreateProductCommandPayload): Promise<Product> {
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
}
