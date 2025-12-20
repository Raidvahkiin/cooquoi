export type SortDirection = "asc" | "desc";

export type SortOption<T extends object> = {
  value: keyof T;
  direction: SortDirection;
};

export interface Filter {
  filterType: string;
}

export interface TextFilter extends Filter {
  filterType: "text";
  type?: string;
  filter?: unknown;
}

export interface SetFilter extends Filter {
  filterType: "set";
  values?: unknown;
}

export type AllFilter = TextFilter | SetFilter;

export type FilterModel<T extends object> = Record<keyof T, AllFilter>;

export type GetManyResult<T> = {
  data: T[];
  total: number;
};

export type GetManyOptions<T extends object> = {
  skip: number;
  limit: number;
  sort?: SortOption<T>;
  filter?: FilterModel<T>;
};
