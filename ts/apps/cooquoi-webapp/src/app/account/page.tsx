import { getTranslations } from 'next-intl/server';
import { getMe } from '@/features/household';

// Spike, phase 1: plain markup; the Astryx shell comes in T02.
export default async function AccountPage() {
  const [me, t, tCommon] = await Promise.all([
    getMe(),
    getTranslations('household.account'),
    getTranslations('common'),
  ]);

  return (
    <main>
      <h1>{t('signedInAs', { name: me.displayName })}</h1>
      <p>{me.email}</p>
      <p>{t('summary', { locale: me.locale, count: me.householdCount })}</p>
      <a href="/auth/logout">{tCommon('signOut')}</a>
    </main>
  );
}
