import { Ingredient, IngredientRepository } from "@cooquoi/domain";
import { IQueryHandler, Query, QueryHandler } from "@nestjs/cqrs";

export class GetIngredientQuery extends Query<Ingredient | null> {
	public readonly ingredientId: string;
	constructor(props: { ingredientId: string }) {
		super();
		this.ingredientId = props.ingredientId;
	}
}

@QueryHandler(GetIngredientQuery)
export class GetIngredientQueryHandler
	implements IQueryHandler<GetIngredientQuery>
{
	constructor(private readonly _ingredientRepository: IngredientRepository) {}

	async execute(query: GetIngredientQuery): Promise<Ingredient | null> {
		return this._ingredientRepository.findById(query.ingredientId);
	}
}
