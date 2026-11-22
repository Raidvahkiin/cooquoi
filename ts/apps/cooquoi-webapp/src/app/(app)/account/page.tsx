import { Heading } from '@astryxdesign/core/Heading';
import { VStack } from '@astryxdesign/core/Stack';
import { Text } from '@astryxdesign/core/Text';
import { getTranslations } from 'next-intl/server';
import { getMe } from '@/features/household';

export default async function AccountPage() {
  const [me, t] = await Promise.all([
    getMe(),
    getTranslations('household.account'),
  ]);

  return (
    <VStack gap={2}>
      <Heading level={1}>{t('signedInAs', { name: me.displayName })}</Heading>
      <Text as="p">{me.email}</Text>
      <Text as="p">
        {t('summary', { locale: me.locale, count: me.householdCount })}
      </Text>
    </VStack>
  );
}
