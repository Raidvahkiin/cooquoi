import type { ProductsEndpoints } from '../../interfaces';
import type { FilterParams, FilterResult } from '../../models/common';
import type {
  CreateProductBody,
  Product,
  ProductWithOffers,
} from '../../models/product';

export class ProductsClient implements ProductsEndpoints {
  constructor(private readonly baseUrl: string) {}

  async create(body: CreateProductBody): Promise<Product> {
    const res = await fetch(`${this.baseUrl}/products`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    return res.json() as Promise<Product>;
  }

  async getMany(
    params?: FilterParams & { maxOffers?: number },
  ): Promise<FilterResult<ProductWithOffers>> {
    const query = params
      ? `?${new URLSearchParams(
          Object.entries(params)
            .filter(([, v]) => v !== undefined)
            .map(([k, v]) => [k, String(v)]),
        ).toString()}`
      : '';
    const res = await fetch(`${this.baseUrl}/products${query}`);
    return res.json() as Promise<FilterResult<ProductWithOffers>>;
  }
}
