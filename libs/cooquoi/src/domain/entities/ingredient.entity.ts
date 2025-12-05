import { Entity, EntityProps, EntityUpdateOptions } from "@libs/core";

export type IngredientProps = EntityProps & {
	name: string;
	description?: string | null;
};

export class Ingredient extends Entity<Ingredient> {
	private _name: string;
	private _description: string | null;

	constructor(props: IngredientProps, options: EntityUpdateOptions) {
		super(props, options);
		this._name = props.name;
		this._description = props.description ?? null;
	}

	get name(): string {
		return this._name;
	}

	get description(): string | null {
		return this._description;
	}

	public update(
		newState: Partial<Omit<IngredientProps, "id" | "createdAt" | "updatedAt">>,
		options: EntityUpdateOptions,
	): void {
		this.commitUpdate({
			newState,
			datetimeProvider: options.datetimeProvider,
		});
	}
}
