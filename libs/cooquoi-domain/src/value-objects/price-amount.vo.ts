import { Currency, Quantity, ValueObject } from "@libs/core";
import { InvalidOperationError } from "../errors";
import { CurrencyExchangeProvider } from "../interfaces";

export class PriceAmount implements ValueObject {
	public readonly amount: Quantity;
	public readonly currency: Currency;

	private constructor(props: { value: Quantity; currency: Currency }) {
		this.amount = props.value;
		this.currency = props.currency;
	}

	static create(value: number, currency: Currency): PriceAmount {
		return new PriceAmount({ value: Quantity.create(value), currency });
	}

	toString(): string {
		return `(${this.currency.symbol}${this.amount})`;
	}

	 
	equals(vo: ValueObject | undefined | null): boolean {
		if (!vo || !(vo instanceof PriceAmount)) {
			return false;
		}

		if (this.currency === vo.currency) {
			return this.amount.equals(vo.amount);
		}

		throw new InvalidOperationError(
			"Cannot compare PriceAmount with different currencies without exchange provider",
		);
	}

		
	compareTo(vo: ValueObject | undefined | null): number {
		if (!vo || !(vo instanceof PriceAmount)) {
			throw new InvalidOperationError(
				"Cannot compare PriceAmount with non-PriceAmount",
			);
		}

		if (this.currency !== vo.currency) {
			throw new InvalidOperationError(
				"Cannot compare PriceAmount with different currencies without exchange provider",
			);
		}

		return this.amount.compareTo(vo.amount);
	}

	async equalsWithExchangeProvider(
		vo: PriceAmount | undefined | null,
		exchangeProvider: CurrencyExchangeProvider,
	): Promise<boolean> {
		if (!vo || !(vo instanceof PriceAmount)) {
			return false;
		}

		if (this.currency === vo.currency) {
			return this.amount.equals(vo.amount);
		}

		const rate = await exchangeProvider.getExchangeRate(
			vo.currency,
			this.currency,
		);
		
		const convertedAmount = vo.amount.multiply(rate);
		return this.amount.equals(convertedAmount);
	}
}
