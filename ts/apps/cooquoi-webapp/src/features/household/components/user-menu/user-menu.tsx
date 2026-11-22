import { Link } from '@astryxdesign/core/Link';
import { HStack } from '@astryxdesign/core/Stack';
import { Text } from '@astryxdesign/core/Text';
import { useTranslations } from 'next-intl';

/** Signed-in user's name and the sign-out link, for the end of the top bar. */
export function UserMenu({ displayName }: { displayName: string }) {
  const t = useTranslations('common');
  return (
    <HStack gap={3}>
      <Text weight="medium">{displayName}</Text>
      <Link href="/auth/logout">{t('signOut')}</Link>
    </HStack>
  );
}
