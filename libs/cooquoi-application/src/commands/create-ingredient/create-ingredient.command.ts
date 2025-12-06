import { Command } from "@nestjs/cqrs";

export class CreateIngredientCommand extends Command<void> {
	public readonly name: string;
	public readonly description: string;

	constructor(props: { name: string; description: string }) {
		super();
		this.name = props.name;
		this.description = props.description;
	}
}
