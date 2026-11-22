'use client';

import { Button } from '@astryxdesign/core/Button';
import { VStack } from '@astryxdesign/core/Stack';
import { useTranslations } from 'next-intl';
import { ErrorMessage } from '@/shared/ui/error-message/error-message';

type ErrorPageProps = {
  reset: () => void;
};

/**
 * Boundary of the whole app (above `(app)/layout.tsx`, whose `getMe` fails when the API is
 * down): the unexpected-error text in the user's language and a retry.
 */
export default function ErrorPage({ reset }: ErrorPageProps) {
  const t = useTranslations('common');
  return (
    <VStack gap={3}>
      <ErrorMessage error={{ code: 'common.unexpected' }} />
      <Button label={t('retry')} onClick={reset} />
    </VStack>
  );
}
