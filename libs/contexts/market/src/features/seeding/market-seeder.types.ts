export interface SeedIngredient {
  name: string;
  description?: string;
  /**
   * Names of other ingredients that compose this one.
   * Example: an "Egg" can be composed of "Egg yolk", "Egg white" and "Egg shell".
   * Referenced ingredients must also be present in `SeedData.ingredients`.
   */
  components?: string[];
}

export interface SeedOffer {
  vendor: string;
  priceAmount: string;
  priceCurrency: string;
}

export interface SeedProduct {
  name: string;
  description?: string;
  ingredients: string[];
  offers: SeedOffer[];
}

export interface SeedData {
  ingredients: SeedIngredient[];
  products: SeedProduct[];
}
