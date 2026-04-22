// Models
export type {
  Ingredient,
  CreateIngredientBody,
  CreateIngredientResponse,
} from './models';
export type {
  Product,
  ProductWithOffers,
  CreateProductBody,
  Offer,
} from './models';
export type { FilterResult, FilterParams } from './models';
export { HealthStatus, type HealthStatusResponse } from './models';
export type { CreateOrUpdateOfferBody } from './models';

// Interfaces
export type { HealthEndpoints } from './interfaces/health-endpoints';
export type { IngredientsEndpoints } from './interfaces/ingredients-endpoint';
export type { OffersEndpoints } from './interfaces/offers-endpoints';
export type { ProductsEndpoints } from './interfaces/products-endpoints';

// HTTP Client
export { CooquoiClient } from './http-client/cooquoi-client';
