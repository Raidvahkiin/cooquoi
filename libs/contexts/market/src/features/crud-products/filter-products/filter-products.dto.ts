import { ApiPropertyOptional } from '@nestjs/swagger';

export class FilterProductsDto {
  @ApiPropertyOptional({
    example: 'salm',
    description:
      'Fuzzy search on product name (ranked first) or ingredient name (ranked second)',
  })
  search?: string;

  @ApiPropertyOptional({
    example: 3,
    description:
      'Maximum number of offers to return per product, sorted by lowest price',
  })
  maxOffers?: number;

  @ApiPropertyOptional({ example: 0, default: 0 })
  skip?: number;

  @ApiPropertyOptional({ example: 20, default: 20 })
  take?: number;

  @ApiPropertyOptional({
    example: 'name',
    enum: ['name', 'createdAt', 'updatedAt'],
  })
  sortField?: string;

  @ApiPropertyOptional({ example: 'asc', enum: ['asc', 'desc'] })
  sortOrder?: 'asc' | 'desc';
}
