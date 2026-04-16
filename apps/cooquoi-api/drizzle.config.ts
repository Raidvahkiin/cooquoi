import { defineConfig } from 'drizzle-kit';

export default defineConfig({
  dialect: 'postgresql',
  schema: [
    '../../libs/contexts/*/src/domain/entities/*.entity.ts',
    '../../libs/contexts/*/src/domain/entities/relations.ts',
  ],
  out: './drizzle',
  dbCredentials: {
    url: process.env.DATABASE_URL!,
  },
});
