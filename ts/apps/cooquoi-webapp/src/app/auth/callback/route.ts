import { signIn } from '@cooquoi/api-client';
import { type NextRequest, NextResponse } from 'next/server';
import { createApiClientWithToken } from '@/shared/api';
import { completeLogin } from '@/shared/auth';
import { getEnv } from '@/shared/config/env';

export async function GET(request: NextRequest) {
  const { returnTo, accessToken } = await completeLogin(request.nextUrl.search);
  // Creates the profile on the first login, refreshes name and email later (ADR 0015).
  await signIn({
    client: createApiClientWithToken(accessToken),
    headers: { 'Idempotency-Key': crypto.randomUUID() },
    throwOnError: true,
  });
  return NextResponse.redirect(new URL(returnTo, getEnv().APP_URL));
}
