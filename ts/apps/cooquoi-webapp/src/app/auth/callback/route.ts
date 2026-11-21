import { type NextRequest, NextResponse } from 'next/server';
import { completeLogin } from '@/shared/auth';
import { getEnv } from '@/shared/config/env';

export async function GET(request: NextRequest) {
  const returnTo = await completeLogin(request.nextUrl.search);
  return NextResponse.redirect(new URL(returnTo, getEnv().APP_URL));
}
