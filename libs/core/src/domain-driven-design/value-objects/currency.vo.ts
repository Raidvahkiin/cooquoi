import { Enumeration, getEnumerationInstanceByValue } from "../../common";

export class Currency extends Enumeration {
	private static registries: Currency[] = [];

	static readonly USD = new Currency("USD", "$");
	static readonly EUR = new Currency("EUR", "€");

	constructor(
		name: string,
		public readonly symbol: string,
	) {
		super(name);
		Currency.registries.push(this);
	}

	override toString(): string {
		return this.symbol;
	}

	static fromValue(value: string): Currency {
		return getEnumerationInstanceByValue(Currency, value);
	}

	static fromSymbol(symbol: string): Currency {
		const currency = Currency.registries.find((c) => c.symbol === symbol);
		if (!currency) {
			throw new Error(`Currency with symbol ${symbol} does not exist`);
		}
		return currency;
	}

	get allInstances(): Currency[] {
		return [...Currency.registries];
	}
}
