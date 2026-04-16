import type { ProductsEndpoints } from '../../interfaces';
import type { CreateProductBody, Product } from '../../models/product';

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
}
