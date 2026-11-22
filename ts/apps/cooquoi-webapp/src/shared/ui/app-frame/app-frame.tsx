import { AppShell } from '@astryxdesign/core/AppShell';
import { TopNav, TopNavHeading, TopNavItem } from '@astryxdesign/core/TopNav';
import { useTranslations } from 'next-intl';
import type { ReactNode } from 'react';

type AppFrameProps = {
  /** End of the top bar, typically the user menu. */
  endContent: ReactNode;
  children: ReactNode;
};

/** Frame of every signed-in page: top navigation and the main content area. */
export function AppFrame({ endContent, children }: AppFrameProps) {
  const t = useTranslations('common');
  return (
    <AppShell
      contentPadding={4}
      topNav={
        <TopNav
          label={t('nav.label')}
          heading={<TopNavHeading heading={t('appName')} headingHref="/" />}
          startContent={
            <TopNavItem label={t('nav.foodpedia')} href="/foodpedia" />
          }
          endContent={endContent}
        />
      }
    >
      {children}
    </AppShell>
  );
}
