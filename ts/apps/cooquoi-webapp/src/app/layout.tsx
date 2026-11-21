import type { ReactNode } from 'react';

export const metadata = {
  title: 'Cooquoi',
  description: 'What to cook and what to buy, from what is in your fridge.',
};

// Spike: bare shell; the UI is rebuilt from scratch with Astryx, next-intl and the theme in phase 4.
export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
