import type { IngredientsEndpoints } from '../../interfaces';
import type { FilterParams, FilterResult } from '../../models/common';
import type { CreateIngredientBody, Ingredient } from '../../models/ingredient';

export class IngredientsClient implements IngredientsEndpoints {
  constructor(private readonly baseUrl: string) {}

  async create(body: CreateIngredientBody): Promise<Ingredient> {
    const res = await fetch(`${this.baseUrl}/ingredients`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    return res.json() as Promise<Ingredient>;
  }

  async getOneById(id: string): Promise<Ingredient> {
    const res = await fetch(`${this.baseUrl}/ingredients/${id}`);
    return res.json() as Promise<Ingredient>;
  }

  async getMany(params?: FilterParams): Promise<FilterResult<Ingredient>> {
    const query = params
      ? `?${new URLSearchParams(
          Object.entries(params)
            .filter(([, v]) => v !== undefined)
            .map(([k, v]) => [k, String(v)]),
        ).toString()}`
      : '';
    const res = await fetch(`${this.baseUrl}/ingredients${query}`);
    return res.json() as Promise<FilterResult<Ingredient>>;
  }

  async delete(id: string): Promise<void> {
    await fetch(`${this.baseUrl}/ingredients/${id}`, { method: 'DELETE' });
  }
}
