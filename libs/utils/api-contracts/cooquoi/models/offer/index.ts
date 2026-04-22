export type CreateOrUpdateOfferBody = {
  productId: string;
  vendor: string;
  price: {
    amount: string;
    currency: string;
  };
};
