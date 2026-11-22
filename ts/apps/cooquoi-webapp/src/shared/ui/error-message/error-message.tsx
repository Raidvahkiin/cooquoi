import { Banner } from '@astryxdesign/core/Banner';
import { useTranslations } from 'next-intl';
import {
  type ApiError,
  UNEXPECTED_ERROR_CODE,
} from '@/shared/api/to-api-error';

type ErrorMessageProps = {
  error: ApiError;
};

/** Shows an API error translated by its code (`errors.*` messages); unknown codes show `common.unexpected`. */
export function ErrorMessage({ error }: ErrorMessageProps) {
  const t = useTranslations('errors');
  const tCommon = useTranslations('common');
  const code = t.has(error.code as never) ? error.code : UNEXPECTED_ERROR_CODE;
  return (
    <Banner
      status="error"
      title={tCommon('errorTitle')}
      description={t(code as never, error.params as never)}
      collapsible={false}
    />
  );
}
