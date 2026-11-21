import { type NextRequest, NextResponse } from 'next/server';
import { startLogin } from '@/shared/auth';

export async function GET(request: NextRequest) {
  const returnTo = request.nextUrl.searchParams.get('returnTo') ?? '/';
  return NextResponse.redirect(await startLogin(returnTo));
}
