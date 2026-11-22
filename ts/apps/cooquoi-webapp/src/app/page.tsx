import { getTranslations } from 'next-intl/server';

// Spike: plain markup until the Astryx rebuild (T02).
export default async function HomePage() {
  const t = await getTranslations('common');
  return (
    <main>
      <h1>{t('appName')}</h1>
      <a href="/account">{t('openAccount')}</a>
    </main>
  );
}
