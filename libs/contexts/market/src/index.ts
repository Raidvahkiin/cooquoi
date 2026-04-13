export { MarketModule } from './market.module';
export type { ModuleConfig } from './config';
export type { Ingredient } from './domain';
export {
  DeleteIngredientCommand,
  CreateIngredientCommand,
  CreateIngredientDto,
  GetIngredientQuery,
  FilterIngredientsQuery,
  FilterIngredientsDto,
  type FilterIngredientsResult,
} from './features';
