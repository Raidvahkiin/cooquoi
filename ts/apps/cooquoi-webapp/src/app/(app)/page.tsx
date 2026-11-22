import { Heading } from '@astryxdesign/core/Heading';
import { Link } from '@astryxdesign/core/Link';
import { getTranslations } from 'next-intl/server';

export default async function HomePage() {
  const t = await getTranslations('common');
  return (
    <>
      <Heading level={1}>{t('appName')}</Heading>
      <Link href="/account">{t('openAccount')}</Link>
    </>
  );
}
