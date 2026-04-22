import type { CreateOrUpdateOfferBody } from '../models/offer';
import type { Offer } from '../models/product';

export interface OffersEndpoints {
  createOrUpdate(body: CreateOrUpdateOfferBody): Promise<Offer>;
  delete(id: string): Promise<void>;
}
