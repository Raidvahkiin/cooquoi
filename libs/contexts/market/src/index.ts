export { MarketModule } from './market.module';
export type { ModuleConfig } from './config';
export type { Ingredient, Offer, Product } from './domain';
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
  FilterProductsQuery,
  FilterProductsDto,
  type FilterProductsResult,
  type ProductWithOffers,
  MarketSeederService,
  type SeedData,
} from './features';
