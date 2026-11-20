import type { FilterParams, FilterResult } from '../models/common';
import type {
  CreateIngredientBody,
  CreateIngredientResponse,
  Ingredient,
} from '../models/ingredient';

export type { CreateIngredientBody, FilterParams };

export interface IngredientsEndpoints {
  create(body: CreateIngredientBody): Promise<CreateIngredientResponse>;
  getOneById(id: string): Promise<Ingredient>;
  getMany(params?: FilterParams): Promise<FilterResult<Ingredient>>;
  delete(id: string): Promise<void>;
}
