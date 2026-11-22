import type { HouseholdMe } from '@cooquoi/api-client';

export const API_BASE_URL = 'http://api.test';

export const meFixture: HouseholdMe = {
  profile: {
    userId: '0198f0a0-0000-7000-8000-000000000001',
    displayName: 'Ada Lovelace',
    email: 'ada@example.test',
    locale: 'fr',
    defaultHouseholdId: '0198f0a0-0000-7000-8000-000000000002',
  },
  households: [
    {
      id: '0198f0a0-0000-7000-8000-000000000002',
      name: null,
      role: 'owner',
    },
  ],
};
