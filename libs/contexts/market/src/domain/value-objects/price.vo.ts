import { z } from 'zod';

export const priceSchema = z.object({
  amount: z.number().nonnegative(),
  currency: z.string().min(3).max(3),
});

export type PriceState = z.infer<typeof priceSchema>;

export class Price {
  private constructor(public readonly state: PriceState) {}

  public get amount(): number {
    return this.state.amount;
  }

  public get currency(): string {
    return this.state.currency;
  }

  static create(data: PriceState): Price {
    const parsed = priceSchema.safeParse(data);
    if (!parsed.success) {
      throw new Error(`Invalid price: ${parsed.error.message}`);
    }
    return new Price(parsed.data);
  }

  toString(): string {
    return `${this.amount} ${this.currency}`;
  }
}
