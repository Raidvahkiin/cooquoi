import { Command } from "@nestjs/cqrs";

export class DeleteIngredientCommand extends Command<void> {
	public readonly id: string;

	constructor(props: { id: string }) {
		super();
		this.id = props.id;
	}
}
