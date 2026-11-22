import { describe, expect, it } from 'vitest';
import { parseIngredientSearchParams } from './ingredient-search.schema';

describe('parseIngredientSearchParams', () => {
  it('reads q, page and sort', () => {
    expect(
      parseIngredientSearchParams({ q: ' saumon ', page: '3', sort: '-name' }),
    ).toEqual({ q: 'saumon', page: 3, sort: '-name' });
  });

  it('defaults an empty URL', () => {
    expect(parseIngredientSearchParams({})).toEqual({
      q: '',
      page: 1,
      sort: undefined,
    });
  });

  it.each([
    ['page', '0'],
    ['page', 'abc'],
    ['page', '1.5'],
    ['sort', 'expiresAt'],
    ['sort', '--name'],
    ['q', 'x'.repeat(101)],
  ])('falls back to the default for an invalid %s (%s)', (key, value) => {
    const parsed = parseIngredientSearchParams({ [key]: value });

    expect(parsed).toEqual({ q: '', page: 1, sort: undefined });
  });

  it('takes the first of repeated values', () => {
    expect(parseIngredientSearchParams({ page: ['2', '5'] }).page).toBe(2);
  });
});
