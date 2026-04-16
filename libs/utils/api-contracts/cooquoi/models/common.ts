export type FilterResult<T> = {
  items: T[];
  total: number;
};

export type FilterParams = {
  skip?: number;
  take?: number;
  search?: string;
  sortField?: string;
  sortOrder?: 'asc' | 'desc';
};
