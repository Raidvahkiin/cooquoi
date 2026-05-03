export { MarketModule } from './market.module';
export type { ModuleConfig } from './config';
export {
  type Ingredient,
  type Offer,
  type Product,
  Price,
  type PriceState,
} from './domain';
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
  DeleteProductCommand,
  FilterProductsQuery,
  type FilterProductsResult,
  type ProductWithOffers,
  CreateOrUpdateOfferCommand,
  type CreateOrUpdateOfferCommandPayload,
  DeleteOfferCommand,
  MarketSeederService,
  type SeedData,
} from './features';
