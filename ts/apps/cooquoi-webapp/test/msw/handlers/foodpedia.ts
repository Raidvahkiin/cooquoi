import { HttpResponse, http } from 'msw';
import { API_BASE_URL } from '../../fixtures/household';
import { ingredientSummaries } from '../../fixtures/foodpedia';

const SUPPORTED = ['en', 'fr', 'vi'] as const;

export const foodpediaHandlers = [
  http.get(`${API_BASE_URL}/v1/ingredients`, ({ request }) => {
    const url = new URL(request.url);
    const accepted = request.headers.get('Accept-Language') ?? 'en';
    const locale = SUPPORTED.find((l) => l === accepted) ?? 'en';
    const items = ingredientSummaries(locale);
    return HttpResponse.json({
      items,
      page: Number(url.searchParams.get('page') ?? 1),
      pageSize: Number(url.searchParams.get('pageSize') ?? 20),
      totalCount: items.length,
    });
  }),
];
