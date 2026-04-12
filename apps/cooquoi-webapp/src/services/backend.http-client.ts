import { appConfig } from '@/config';
import type {
  CreateIngredientDto,
  FilterIngredientsResult,
  IngredientDto,
} from '@/types/ingredient';

export type FilterIngredientsRequest = {
  skip: number;
  take: number;
  search?: string;
};

class BackendClient {
  private readonly baseUrl: string;

  constructor() {
    this.baseUrl = appConfig.backend.baseUrl.replace(/\/$/, '');
  }

  private url(path: string): string {
    return `${this.baseUrl}/${path}`;
  }

  async filterIngredients(
    params: FilterIngredientsRequest,
  ): Promise<FilterIngredientsResult> {
    try {
      const query = new URLSearchParams({
        skip: String(params.skip),
        take: String(params.take),
      });
      if (params.search) query.set('search', params.search);
      const response = await fetch(`${this.url('ingredients')}?${query}`);
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      return response.json() as Promise<FilterIngredientsResult>;
    } catch (error) {
      const err = error instanceof Error ? error : new Error(String(error));
      console.error('Error filtering ingredients:', err.message, err.stack);
      return { items: [], total: 0 };
    }
  }

  async createIngredient(dto: CreateIngredientDto): Promise<IngredientDto> {
    const response = await fetch(this.url('ingredients'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto),
    });
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    return response.json() as Promise<IngredientDto>;
  }
}

const instance = new BackendClient();

export const backendClient = instance;
