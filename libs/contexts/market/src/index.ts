export { MarketModule } from './market.module';
export type { ModuleConfig } from './config';
export type { Ingredient, Product } from './domain';
export {
  CreateIngredientCommand,
  type CreateIngredientDto,
  GetIngredientQuery,
  FilterIngredientsQuery,
  FilterIngredientsDto,
  DeleteIngredientCommand,
  type FilterIngredientsResult,
  CreateProductCommand,
  type CreateProductCommandPayload,
} from './features';
