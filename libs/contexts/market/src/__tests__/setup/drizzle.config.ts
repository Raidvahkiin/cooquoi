import { defineConfig } from 'drizzle-kit';

export default defineConfig({
  dialect: 'postgresql',
  schema: '../../domain/entities/ingredient.entity.ts',
  out: './drizzle',
});
