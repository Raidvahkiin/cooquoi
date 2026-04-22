import type { FilterParams } from './common';

export const endpointPaths = {
  health: {
    status: () => '/health',
  },
  ingredients: {
    create: () => '/ingredients',
    getOneById: (id: string) => `/ingredients/${id}`,
    getMany: (params: FilterParams) => {
      const query = params ? buildQueryString(params) : '';
      return `/ingredients${query}`;
    },
    delete: (id: string) => `/ingredients/${id}`,
  },
  products: {
    create: () => '/products',
    getOneById: (id: string) => `/products/${id}`,
    getMany: (params: FilterParams) => {
      const query = params ? buildQueryString(params) : '';
      return `/products${query}`;
    },
    delete: (id: string) => `/products/${id}`,
  },
  offers: {
    createOrUpdate: () => '/offers',
    delete: (id: string) => `/offers/${id}`,
  },
};

function buildQueryString(params: FilterParams) {
  return `?${new URLSearchParams(
    Object.entries(params)
      .filter(([, v]) => v !== undefined)
      .map(([k, v]) => [k, String(v)]),
  ).toString()}`;
}
