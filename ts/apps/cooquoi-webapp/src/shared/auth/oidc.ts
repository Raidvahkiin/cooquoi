import 'server-only';
import * as oidc from 'openid-client';
import { getEnv } from '@/shared/config/env';

let configPromise: Promise<oidc.Configuration> | undefined;

/** Keycloak discovery, done once per server process. */
export function getOidcConfig(): Promise<oidc.Configuration> {
  configPromise ??= discoverKeycloak().catch((error: unknown) => {
    configPromise = undefined;
    throw error;
  });
  return configPromise;
}

async function discoverKeycloak(): Promise<oidc.Configuration> {
  const env = getEnv();
  const issuer = new URL(env.OIDC_ISSUER);
  // Local Keycloak runs over plain HTTP; production always uses HTTPS.
  const options =
    issuer.protocol === 'http:'
      ? { execute: [oidc.allowInsecureRequests] }
      : undefined;
  return oidc.discovery(
    issuer,
    env.OIDC_CLIENT_ID,
    env.OIDC_CLIENT_SECRET,
    undefined,
    options,
  );
}
