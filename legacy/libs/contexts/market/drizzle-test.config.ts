import { defineConfig } from 'drizzle-kit';

export default defineConfig({
  dialect: 'postgresql',
  schema: [
    './src/domain/entities/*.entity.ts',
    './src/domain/entities/relations.ts',
  ],
  out: './src/__tests__/setup/drizzle',
  extensionsFilters: ['postgis'],
});
