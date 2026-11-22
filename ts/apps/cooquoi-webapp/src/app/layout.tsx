import { getLocale, getMessages } from 'next-intl/server';
import { NextIntlClientProvider } from 'next-intl';
import type { ReactNode } from 'react';

export const metadata = {
  title: 'Cooquoi',
  description: 'What to cook and what to buy, from what is in your fridge.',
};

// Spike: bare shell; the Astryx theme comes in T02.
export default async function RootLayout({
  children,
}: {
  children: ReactNode;
}) {
  const [locale, messages] = await Promise.all([getLocale(), getMessages()]);
  return (
    <html lang={locale}>
      <body>
        <NextIntlClientProvider locale={locale} messages={messages}>
          {children}
        </NextIntlClientProvider>
      </body>
    </html>
  );
}
