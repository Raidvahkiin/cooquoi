import type { CreateProductBody, Product } from '../models/product';

export type { CreateProductBody };

export interface ProductsEndpoints {
  create(body: CreateProductBody): Promise<Product>;
}
