import { Entity, EntityProps, EntityUpdateOptions, Quantity } from "@libs/core";
import { Ingredient } from "./ingredient.entity";

export type RecipeProps = EntityProps & {
	name: string;
	description?: string | null;
	ingredients?: Map<Ingredient, Quantity>;
};

export class Recipe extends Entity<Recipe> {
	private _name: string;
	private _description: string | null;
	private _ingredients: Map<Ingredient, Quantity>;

	constructor(props: RecipeProps, options: EntityUpdateOptions) {
		super(props, options);
		this._name = props.name;
		this._description = props.description ?? null;
		this._ingredients = props.ingredients ?? new Map<Ingredient, Quantity>();
	}

	// # region Getters
	get name(): string {
		return this._name;
	}

	get description(): string | null {
		return this._description;
	}

	get ingredients(): Map<Ingredient, Quantity> {
		return new Map(this._ingredients);
	}
	// # endregion

	public update(
		newState: Partial<Omit<RecipeProps, "id" | "createdAt" | "updatedAt">>,
		options: EntityUpdateOptions,
	): void {
		this.commitUpdate({
			newState,
			datetimeProvider: options.datetimeProvider,
		});
	}
}
