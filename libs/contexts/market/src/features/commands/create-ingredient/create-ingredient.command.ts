export class CreateIngredientCommand {
  constructor(
    public readonly name: string,
    public readonly description?: string,
  ) {}
}
