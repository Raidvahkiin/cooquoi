import '@astryxdesign/core/reset.css';
import '@astryxdesign/core/astryx.css';
import '@/shared/ui/theme/cooquoi.css';
import { Theme } from '@astryxdesign/core/theme';
import { type AbstractIntlMessages, NextIntlClientProvider } from 'next-intl';
import type { ReactNode } from 'react';
import { render } from 'vitest-browser-react';
import { cooquoiTheme } from '@/shared/ui/theme/cooquoi';
import en from '../messages/en.json';

type RenderWithProvidersOptions = {
  locale?: string;
  messages?: AbstractIntlMessages;
};

/** Renders a component as the app does: next-intl (English by default) and the `cooquoi` theme. */
export function renderWithProviders(
  ui: ReactNode,
  { locale = 'en', messages = en }: RenderWithProvidersOptions = {},
) {
  return render(
    <NextIntlClientProvider locale={locale} messages={messages}>
      <Theme theme={cooquoiTheme}>{ui}</Theme>
    </NextIntlClientProvider>,
  );
}
