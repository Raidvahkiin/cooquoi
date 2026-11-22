import { describe, expect, it } from 'vitest';
import {
  toEffectiveSort,
  toNextSort,
  toSearchHref,
} from './ingredient-search.view-model';

describe('toEffectiveSort', () => {
  it('defaults to relevance with a query and to name without', () => {
    expect(toEffectiveSort({ q: 'sal' })).toEqual({
      key: 'relevance',
      direction: 'descending',
    });
    expect(toEffectiveSort({ q: '' })).toEqual({
      key: 'name',
      direction: 'ascending',
    });
  });

  it('reads an explicit descending sort', () => {
    expect(toEffectiveSort({ q: '', sort: '-code' })).toEqual({
      key: 'code',
      direction: 'descending',
    });
  });
});

describe('toNextSort', () => {
  it('sorts ascending first, then flips', () => {
    const name = { key: 'name', direction: 'ascending' } as const;
    expect(toNextSort({ current: name, key: 'name' })).toBe('-name');
    expect(toNextSort({ current: name, key: 'code' })).toBe('code');
    expect(
      toNextSort({
        current: { key: 'name', direction: 'descending' },
        key: 'name',
      }),
    ).toBe('name');
  });
});

describe('toSearchHref', () => {
  it('leaves defaults out', () => {
    expect(toSearchHref({ q: '', page: 1 })).toBe('/foodpedia');
    expect(toSearchHref({ q: 'cá', sort: 'name', page: 2 })).toBe(
      '/foodpedia?q=c%C3%A1&sort=name&page=2',
    );
  });
});
