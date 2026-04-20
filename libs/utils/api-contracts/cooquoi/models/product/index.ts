export type Offer = {
  id: string;
  productId: string;
  vendor: string;
  priceAmount: string;
  priceCurrency: string;
  createdAt: string;
  updatedAt: string;
};

export type Product = {
  id: string;
  name: string;
  description: string | null;
  createdAt: string;
  updatedAt: string;
};

export type ProductWithOffers = Product & { offers: Offer[] };

export type CreateProductBody = {
  name: string;
  description?: string;
  ingredients: string[];
};
