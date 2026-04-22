import type { OffersEndpoints } from '../../interfaces';
import type { CreateOrUpdateOfferBody } from '../../models/offer';
import type { Offer } from '../../models/product';

export class OffersClient implements OffersEndpoints {
  constructor(private readonly baseUrl: string) {}

  async createOrUpdate(body: CreateOrUpdateOfferBody): Promise<Offer> {
    const res = await fetch(`${this.baseUrl}/offers`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    return res.json() as Promise<Offer>;
  }

  async delete(id: string): Promise<void> {
    await fetch(`${this.baseUrl}/offers/${id}`, { method: 'DELETE' });
  }
}
