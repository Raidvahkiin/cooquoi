import { NextResponse } from 'next/server';
import { logout } from '@/shared/auth';

export async function GET() {
  return NextResponse.redirect(await logout());
}
