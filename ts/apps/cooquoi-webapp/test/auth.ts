import { NextRequest } from 'next/server';
import { GET as startLogin } from '@/app/auth/login/route';

export const APP_URL = 'http://localhost:3000';

export function appRequest(path: string): NextRequest {
  return new NextRequest(new URL(path, APP_URL));
}

/** Runs the login route like a browser would and returns the Keycloak redirect. */
export async function startLoginAt(path: string): Promise<URL> {
  const response = await startLogin(appRequest(path));
  return new URL(response.headers.get('location') ?? '');
}
