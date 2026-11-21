import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';

export class GetIngredientsRequestDto {
  @ApiPropertyOptional({
    example: 'tom',
    description: 'Fuzzy search on ingredient name',
  })
  search?: string;

  @ApiPropertyOptional({ example: 0, default: 0 })
  @Type(() => Number)
  skip?: number;

  @ApiPropertyOptional({ example: 20, default: 20 })
  @Type(() => Number)
  take?: number;

  @ApiPropertyOptional({
    example: 'name',
    enum: ['name', 'createdAt', 'updatedAt'],
  })
  sortField?: string;

  @ApiPropertyOptional({ example: 'asc', enum: ['asc', 'desc'] })
  sortOrder?: 'asc' | 'desc';
}
