import { Entity, EntityProps, EntityUpdateOptions } from "@libs/core";
import { ProductCreationError } from "../errors";
import { IngredientAmount } from "../value-objects";
import { Ingredient } from "./ingredient.entity";

export type ProductProps = EntityProps & {
	name: string;
	ingredients: Map<Ingredient, IngredientAmount>;
	aliases?: string[];
	customAttributes?: Record<string, string | number | boolean>;
};

export class Product extends Entity<Product> {
	private _name: string;
	private _aliases: string[];
	private _ingredients: Map<Ingredient, IngredientAmount>;
	private _customAttributes: Record<string, string | number | boolean>;

	constructor(props: ProductProps, options?: EntityUpdateOptions) {
		super(props, options);

		if (!props.ingredients || props.ingredients.size === 0) {
			throw new ProductCreationError(
				"Product must have at least one ingredient",
			);
		}

		this._name = props.name;
		this._aliases = props.aliases ?? [];
		this._ingredients = props.ingredients;
		this._customAttributes = props.customAttributes ?? {};
	}

	get name(): string {
		return this._name;
	}
	get aliases(): string[] {
		return [...this._aliases];
	}

	get ingredients(): Map<Ingredient, IngredientAmount> {
		return new Map(this._ingredients);
	}

	get customAttributes() {
		return { ...this._customAttributes };
	}

	public update(
		newState: Partial<Omit<ProductProps, "id" | "createdAt" | "updatedAt">>,
		options: EntityUpdateOptions,
	): void {
		this.commitUpdate({
			newState,
			datetimeProvider: options.datetimeProvider,
		});
	}
}
