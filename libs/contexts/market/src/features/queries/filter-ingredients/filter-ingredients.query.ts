export class FilterIngredientsQuery {
  constructor(
    public readonly skip: number,
    public readonly take: number,
    public readonly search?: string,
  ) {}
}
