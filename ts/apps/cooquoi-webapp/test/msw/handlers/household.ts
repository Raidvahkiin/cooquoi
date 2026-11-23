import { HttpResponse, http } from 'msw';
import { API_BASE_URL, meFixture } from '../../fixtures/household';

export const householdHandlers = [
  http.get(`${API_BASE_URL}/v1/me`, () => HttpResponse.json(meFixture)),
  // `\\:` escapes the colon, which MSW would read as a path parameter.
  http.post(
    `${API_BASE_URL}/v1/me\\:sign-in`,
    () => new HttpResponse(null, { status: 204 }),
  ),
];
