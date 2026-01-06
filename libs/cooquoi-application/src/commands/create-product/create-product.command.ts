import { IngredientAmount } from "@cooquoi/domain";
import { Command } from "@nestjs/cqrs";

export type ProductIngredient = {
	ingredientId: string;
	quantity: IngredientAmount;
};

export class CreateProductCommand extends Command<void> {
	public readonly name: string;
	public readonly aliases?: string[];
	public readonly ingredients: ProductIngredient[];
	public readonly customAttributes?: Record<string, string | number | boolean>;

	constructor(props: {
		name: string;
		aliases?: string[];
		ingredients: ProductIngredient[];
		customAttributes?: Record<string, string | number | boolean>;
	}) {
		super();
		this.name = props.name;
		this.aliases = props.aliases;
		this.ingredients = props.ingredients;
		this.customAttributes = props.customAttributes;
	}
}
