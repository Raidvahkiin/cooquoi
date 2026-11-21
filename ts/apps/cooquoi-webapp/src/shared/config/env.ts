import 'server-only';
import { z } from 'zod';

const envSchema = z.object({
  APP_URL: z.url(),
  API_BASE_URL: z.url(),
  OIDC_ISSUER: z.url(),
  OIDC_CLIENT_ID: z.string().min(1),
  OIDC_CLIENT_SECRET: z.string().min(1),
  SESSION_PASSWORD: z.string().min(32),
});

export type Env = z.infer<typeof envSchema>;

let cachedEnv: Env | undefined;

/** Parsed once: a missing or invalid variable fails on first use with a clear message. */
export function getEnv(): Env {
  cachedEnv ??= envSchema.parse(process.env);
  return cachedEnv;
}
