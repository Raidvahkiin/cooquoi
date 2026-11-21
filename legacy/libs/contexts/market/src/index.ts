export type { ModuleConfig } from './config';
export {
  type Ingredient,
  type Offer,
  Price,
  type PriceState,
  type Product,
} from './domain';
export {
  CreateIngredientCommand,
  type CreateIngredientDto,
  CreateOrUpdateOfferCommand,
  type CreateOrUpdateOfferCommandPayload,
  CreateProductCommand,
  type CreateProductCommandPayload,
  DeleteIngredientCommand,
  DeleteOfferCommand,
  DeleteProductCommand,
  FilterIngredientsDto,
  FilterIngredientsQuery,
  type FilterIngredientsResult,
  FilterProductsQuery,
  type FilterProductsResult,
  GetIngredientQuery,
  MarketSeederService,
  type ProductWithOffers,
  type SeedData,
} from './features';
export { MarketModule } from './market.module';
