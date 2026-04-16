import { defineConfig } from 'drizzle-kit';

export default defineConfig({
  dialect: 'postgresql',
  schema: [
    './src/domain/entities/ingredient.entity.ts',
    './src/domain/entities/product.entity.ts',
    './src/domain/entities/offer.entity.ts',
    './src/domain/entities/relations.ts',
  ],
  out: './src/__tests__/setup/drizzle',
  extensionsFilters: ['postgis'],
});
