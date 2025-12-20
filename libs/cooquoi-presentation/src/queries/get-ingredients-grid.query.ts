import { IngredientGetter, IngredientModel } from "@cooquoi/infrastructure";
import { GetManyOptions } from "@libs/core";
import { IQueryHandler, Query, QueryHandler } from "@nestjs/cqrs";

export type IngredientsGridResult = {
  rows: IngredientModel[];
  lastRow: number;
};

export class GetIngredientsGridQuery extends Query<IngredientsGridResult> {
  public readonly options: GetManyOptions<IngredientModel>;

  constructor(props: GetManyOptions<IngredientModel>) {
    super();
    this.options = props;
  }
}

@QueryHandler(GetIngredientsGridQuery)
export class GetIngredientsGridQueryHandler implements IQueryHandler<GetIngredientsGridQuery> {
  constructor(private readonly getter: IngredientGetter) {}

  async execute({
    options,
  }: GetIngredientsGridQuery): Promise<IngredientsGridResult> {
    const result = await this.getter.getMany(options);

    return {
      rows: result.data,
      lastRow: result.total,
    };
  }
}
