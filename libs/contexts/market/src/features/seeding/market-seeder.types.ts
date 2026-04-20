export interface SeedIngredient {
  name: string;
  description?: string;
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
