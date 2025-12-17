import { Ingredient, IngredientRepository } from "@cooquoi/domain";
import { EntityFilter } from "@libs/core";
import { IQueryHandler, Query, QueryHandler } from "@nestjs/cqrs";

export class GetManyIngredientsQuery extends Query<Ingredient[]> {
	public readonly filters: EntityFilter<Ingredient>[];
	constructor(props: { filters: EntityFilter<Ingredient>[] }) {
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
