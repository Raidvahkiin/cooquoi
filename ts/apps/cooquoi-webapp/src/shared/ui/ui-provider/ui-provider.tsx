'use client';

import '@astryxdesign/core/reset.css';
import '@astryxdesign/core/astryx.css';
import '../theme/cooquoi.css';
import { LinkProvider } from '@astryxdesign/core/Link';
import { Theme } from '@astryxdesign/core/theme';
import Link from 'next/link';
import type { ComponentProps, ReactNode } from 'react';
import { cooquoiTheme } from '../theme/cooquoi';

/** Route handlers (`/auth/*`) get a plain anchor: next/link would prefetch them, and logout must never run on prefetch. */
function RouterLink({ href, ...props }: ComponentProps<typeof Link>) {
  if (typeof href === 'string' && href.startsWith('/auth/')) {
    return <a href={href} {...props} />;
  }
  return <Link href={href} {...props} />;
}

/** Astryx theme (light/dark from the OS) and Next.js routing for every Astryx link. */
export function UiProvider({ children }: { children: ReactNode }) {
  return (
    <Theme theme={cooquoiTheme} mode="system">
      <LinkProvider component={RouterLink}>{children}</LinkProvider>
    </Theme>
  );
}
