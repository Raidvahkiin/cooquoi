import type { FilterParams, FilterResult } from '../models/common';
import type {
  CreateProductBody,
  Product,
  ProductWithOffers,
} from '../models/product';

export interface ProductsEndpoints {
  create(body: CreateProductBody): Promise<Product>;
  getMany(
    params?: FilterParams & { maxOffers?: number },
  ): Promise<FilterResult<ProductWithOffers>>;
}
