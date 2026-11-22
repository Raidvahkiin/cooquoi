import 'server-only';
import { z } from 'zod';

const envSchema = z.object({
  APP_URL: z.url(),
  API_BASE_URL: z.url(),
  /** Upper bound of one API call, so a stopped API fails fast instead of hanging the page. */
  COOQUOI_API_TIMEOUT_MS: z.coerce.number().int().positive().default(5000),
  OIDC_ISSUER: z.url({ protocol: /^https$/ }),
  OIDC_CLIENT_ID: z.string().min(1),
  OIDC_CLIENT_SECRET: z.string().min(1),
  SESSION_PASSWORD: z.string().min(32),
  /** Session store; carries the webapp's ACL user and password. */
  SESSION_STORE_URL: z.url({ protocol: /^rediss?$/ }),
});

export type Env = z.infer<typeof envSchema>;

let cachedEnv: Env | undefined;

/** Parsed once: a missing or invalid variable fails on first use with a clear message. */
export function getEnv(): Env {
  cachedEnv ??= envSchema.parse(process.env);
  return cachedEnv;
}
