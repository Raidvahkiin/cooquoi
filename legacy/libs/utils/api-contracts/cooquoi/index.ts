// Models

// HTTP Client
export { CooquoiClient } from './http-client/cooquoi-client';
// Interfaces
export type { HealthEndpoints } from './interfaces/health-endpoints';
export type { IngredientsEndpoints } from './interfaces/ingredients-endpoint';
export type { OffersEndpoints } from './interfaces/offers-endpoints';
export type { ProductsEndpoints } from './interfaces/products-endpoints';
export type {
  CreateIngredientBody,
  CreateIngredientResponse,
  CreateOrUpdateOfferBody,
  CreateProductBody,
  FilterParams,
  FilterResult,
  Ingredient,
  Offer,
  Product,
  ProductWithOffers,
} from './models';
export { HealthStatus, type HealthStatusResponse } from './models';
