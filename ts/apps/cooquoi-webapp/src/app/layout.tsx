import type { Metadata } from 'next';
import { NextIntlClientProvider } from 'next-intl';
import { getLocale, getTranslations } from 'next-intl/server';
import type { ReactNode } from 'react';
import { bodyFont, UiProvider } from '@/shared/ui';

export async function generateMetadata(): Promise<Metadata> {
  const t = await getTranslations('common.metadata');
  return { title: t('title'), description: t('description') };
}

export default async function RootLayout({
  children,
}: {
  children: ReactNode;
}) {
  const locale = await getLocale();
  return (
    <html lang={locale} className={bodyFont.variable}>
      <body>
        <NextIntlClientProvider>
          <UiProvider>{children}</UiProvider>
        </NextIntlClientProvider>
      </body>
    </html>
  );
}
