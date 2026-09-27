import { Query } from '@nestjs/cqrs';
import { Ingredient } from '../../../domain';

export class GetIngredientQuery extends Query<Ingredient | null> {
  constructor(public readonly id: string) {
    super();
  }
}
