import {
  HealthClient,
  IngredientsClient,
  OffersClient,
  ProductsClient,
} from './services';

export class CooquoiClient {
  readonly health: HealthClient;
  readonly ingredients: IngredientsClient;
  readonly offers: OffersClient;
  readonly products: ProductsClient;

  constructor(baseUrl: string) {
    this.health = new HealthClient(baseUrl);
    this.ingredients = new IngredientsClient(baseUrl);
    this.offers = new OffersClient(baseUrl);
    this.products = new ProductsClient(baseUrl);
  }
}
