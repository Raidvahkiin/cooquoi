export interface FilterProductsQueryParams {
  skip: number;
  take: number;
  search?: string;
  maxOffers?: number;
  sort?: {
    field: string;
    order: 'asc' | 'desc';
  };
}

export class FilterProductsQuery {
  constructor(public readonly params: FilterProductsQueryParams) {}
}
