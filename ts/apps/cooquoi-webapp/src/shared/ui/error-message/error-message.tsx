import { Banner } from '@astryxdesign/core/Banner';
import { useTranslations } from 'next-intl';
import {
  type ApiError,
  UNEXPECTED_ERROR_CODE,
} from '@/shared/api/to-api-error';

type TranslateByCode = {
  (code: string, params?: ApiError['params']): string;
  has: (code: string) => boolean;
};

type ErrorMessageProps = {
  error: ApiError;
};

/** Shows an API error translated by its code (`errors.*` messages); unknown codes show `common.unexpected`. */
export function ErrorMessage({ error }: ErrorMessageProps) {
  const t = useTranslations('errors');
  const tCommon = useTranslations('common');
  // Codes arrive at run time, so next-intl's typed keys cannot check them: one cast, here.
  const translate = t as unknown as TranslateByCode;
  const code = translate.has(error.code) ? error.code : UNEXPECTED_ERROR_CODE;
  return (
    <Banner
      status="error"
      title={tCommon('errorTitle')}
      description={translate(code, error.params)}
      collapsible={false}
    />
  );
}
