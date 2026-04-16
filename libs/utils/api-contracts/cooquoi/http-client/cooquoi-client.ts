import { HealthClient, IngredientsClient, ProductsClient } from './services';

export class CooquoiClient {
  readonly health: HealthClient;
  readonly ingredients: IngredientsClient;
  readonly products: ProductsClient;

  constructor(baseUrl: string) {
    this.health = new HealthClient(baseUrl);
    this.ingredients = new IngredientsClient(baseUrl);
    this.products = new ProductsClient(baseUrl);
  }
}
