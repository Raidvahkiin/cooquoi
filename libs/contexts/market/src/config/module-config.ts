import type { NodePgDatabase } from 'drizzle-orm/node-postgres';
import { z } from 'zod';
import type { schema } from '../domain';

export const DATABASE_TOKEN = Symbol('MARKET_DATABASE');

export type MarketDatabase = NodePgDatabase<typeof schema>;

export const moduleConfigSchema = z.object({
  database: z.object({
    url: z.string().nonempty(),
  }),
});

export type ModuleConfig = z.infer<typeof moduleConfigSchema>;
