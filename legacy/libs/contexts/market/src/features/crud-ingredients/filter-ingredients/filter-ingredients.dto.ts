export class FilterIngredientsDto {
  search?: string;
  skip?: number;
  take?: number;
  sortField?: string;
  sortOrder?: 'asc' | 'desc';
}
