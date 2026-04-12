import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';

export class CreateIngredientDto {
  @ApiProperty({ example: 'Tomato' })
  name!: string;

  @ApiPropertyOptional({ example: 'A red fruit used in salads' })
  description?: string;
}
