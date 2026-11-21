import { ValueObject } from './value-object';

export class Price implements ValueObject {
  private constructor(
    public readonly amount: number,
    public readonly currency: string,
  ) {}

  static create(amount: number, currency: string): Price {
    if (amount < 0) {
      throw new Error('Price amount cannot be negative');
    }
    if (!currency) {
      throw new Error('Currency cannot be empty');
    }
    return new Price(amount, currency);
  }

  toString(): string {
    return `${this.amount} ${this.currency}`;
  }

  static fromString(value: string): Price {
    const [amountStr, currency] = value.split(' ');
    const amount = Number.parseFloat(amountStr);
    if (Number.isNaN(amount)) {
      throw new Error(`Invalid price amount: ${amountStr}`);
    }
    return Price.create(amount, currency);
  }

  equals(vo: ValueObject | undefined | null): boolean {
    if (vo === null || vo === undefined) {
      return false;
    }
    if (!(vo instanceof Price)) {
      return false;
    }
    return this.amount === vo.amount && this.currency === vo.currency;
  }

  compareTo(vo: ValueObject | undefined | null): number {
    if (vo === null || vo === undefined) {
      return 1;
    }
    if (!(vo instanceof Price)) {
      throw new Error('Cannot compare Price with non-Price value object');
    }
    if (this.amount < vo.amount) {
      return -1;
    }
    if (this.amount > vo.amount) {
      return 1;
    }
    return 0;
  }
}
