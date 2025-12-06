import { Ingredient, IngredientRepository } from "@cooquoi/domain";
import { IQueryHandler, Query, QueryHandler } from "@nestjs/cqrs";

export class GetManyIngredientsQuery extends Query<Ingredient[]> {
	public readonly filters: string[];
	constructor(props: { filters: string[] }) {
		super();
		this.filters = props.filters;
	}
}

@QueryHandler(GetManyIngredientsQuery)
export class GetManyIngredientsQueryHandler
	implements IQueryHandler<GetManyIngredientsQuery>
{
	constructor(private readonly _ingredientRepository: IngredientRepository) {}

	async execute({ filters }: GetManyIngredientsQuery): Promise<Ingredient[]> {
		return this._ingredientRepository.findMany(filters);
	}
}
