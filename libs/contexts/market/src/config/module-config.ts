import { z } from 'zod';

export const DATABASE_TOKEN = Symbol('MARKET_DATABASE');

export const moduleConfigSchema = z.object({
  database: z.object({
    url: z.string().nonempty(),
  }),
});

export type ModuleConfig = z.infer<typeof moduleConfigSchema>;
