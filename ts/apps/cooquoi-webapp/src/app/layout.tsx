import { NextIntlClientProvider } from 'next-intl';
import { getLocale } from 'next-intl/server';
import type { ReactNode } from 'react';
import { bodyFont, UiProvider } from '@/shared/ui';

export const metadata = {
  title: 'Cooquoi',
  description: 'What to cook and what to buy, from what is in your fridge.',
};

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
