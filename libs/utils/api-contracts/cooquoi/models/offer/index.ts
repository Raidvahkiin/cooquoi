export type CreateOrUpdateOfferBody = {
  productId: string;
  vendor: string;
  price: {
    amount: number;
    currency: string;
  };
};
