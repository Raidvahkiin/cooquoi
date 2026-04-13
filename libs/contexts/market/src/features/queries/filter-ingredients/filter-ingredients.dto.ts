import { ApiPropertyOptional } from '@nestjs/swagger';

export class FilterIngredientsDto {
  @ApiPropertyOptional({
    example: 'tom',
    description: 'Fuzzy search on ingredient name',
  })
  search?: string;

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
