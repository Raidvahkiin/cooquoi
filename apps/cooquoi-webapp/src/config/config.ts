export const appConfig = {
  backend: {
    baseUrl: process.env.API_BASE_URL || 'http://localhost:3001',
  },
} as const;
