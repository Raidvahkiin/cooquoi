import { Entity, EntityProps, EntityUpdateOptions } from "@libs/core";
import { Vendor } from "./vendor.entity";
import { Product } from "./product.entity";
import { PriceAmount } from "../value-objects/price-amount.vo";

export type OfferProps = EntityProps & {
	vendor: Vendor;
	product: Product;
	price: PriceAmount;
};

export class Offer extends Entity<Offer> {
	private _vendor: Vendor;
	private _product: Product;
	private _price: PriceAmount;

	constructor(props: OfferProps, options?: EntityUpdateOptions) {
		super(props, options);
		this._vendor = props.vendor;
		this._product = props.product;
		this._price = props.price;
	}

	get vendor(): Vendor {
		return this._vendor;
	}

	get product(): Product {
		return this._product;
	}

	get price(): PriceAmount {
		return this._price;
	}

	public update(
		newState: Partial<Omit<OfferProps, "id" | "createdAt" | "updatedAt">>,
		options: EntityUpdateOptions,
	): void {
		this.commitUpdate({
			newState,
			datetimeProvider: options.datetimeProvider,
		});
	}
}
