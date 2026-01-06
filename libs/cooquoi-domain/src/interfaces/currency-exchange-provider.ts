import { Currency } from "@libs/core";

export abstract class CurrencyExchangeProvider {
    abstract getExchangeRate(
        fromCurrency: Currency,
        toCurrency: Currency,
    ): Promise<number>;
}