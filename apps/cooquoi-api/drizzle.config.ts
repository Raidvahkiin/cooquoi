import { defineConfig } from 'drizzle-kit';
import { config } from 'dotenv';

config({ path: '.env.local' });

console.log('Using database URL:', process.env.DATABASE_URL);

export default defineConfig({
  dialect: 'postgresql',
  schema: [
    '../../libs/contexts/*/src/domain/entities/*.entity.ts',
    '../../libs/contexts/*/src/domain/entities/relations.ts',
  ],
  out: './drizzle',
  dbCredentials: {
    url: process.env.DATABASE_URL ?? '',
  },
});
