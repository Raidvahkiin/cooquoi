import { Entity } from "@libs/core";

export class Ingredient extends Entity<Ingredient> {
	private _name: string;

	constructor(id: string, name: string) {
		super(id);
		this._name = name;
	}

	get name(): string {
		return this._name;
	}
}
