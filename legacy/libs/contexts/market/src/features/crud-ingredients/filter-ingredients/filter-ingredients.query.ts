export interface FilterIngredientsQueryParams {
  skip: number;
  take: number;
  search?: string;
  sort?: {
    field: string;
    order: 'asc' | 'desc';
  };
}

export class FilterIngredientsQuery {
  constructor(public readonly params: FilterIngredientsQueryParams) {}
}
