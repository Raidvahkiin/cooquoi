import type { ReactNode } from 'react';
import { getMe, UserMenu } from '@/features/household';
import { requireAccessToken } from '@/shared/auth';
import { AppFrame } from '@/shared/ui';

export default async function AppLayout({ children }: { children: ReactNode }) {
  await requireAccessToken();
  const me = await getMe();
  return (
    <AppFrame endContent={<UserMenu displayName={me.displayName} />}>
      {children}
    </AppFrame>
  );
}
