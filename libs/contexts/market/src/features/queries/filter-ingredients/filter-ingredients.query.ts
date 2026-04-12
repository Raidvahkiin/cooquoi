export interface FilterIngredientsQueryParams {
  skip: number;
  take: number;
  search?: string;
}

export class FilterIngredientsQuery {
  constructor(public readonly params: FilterIngredientsQueryParams) {}
}
