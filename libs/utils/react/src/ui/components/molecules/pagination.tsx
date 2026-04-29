import type { ComponentType, FC } from 'react';
import { Button } from '../atoms/button';
import { Stack } from '../atoms/stack';

type LinkProps = {
  href: string;
  className?: string;
  children?: React.ReactNode;
};

type Props = {
  page: number;
  totalPages: number;
  total: number;
  prevHref?: string;
  nextHref?: string;
  /**
   * Provide your router's Link component (e.g. Next.js `<Link>`).
   * Falls back to a plain `<a>` if omitted.
   */
  LinkComponent?: ComponentType<LinkProps>;
};

const defaultLinkClass =
  'inline-flex items-center justify-center rounded px-3 py-1.5 text-sm border border-neutral-500 text-neutral-500 hover:bg-neutral-100 transition-colors';

export const Pagination: FC<Props> = ({
  page,
  totalPages,
  total,
  prevHref,
  nextHref,
  LinkComponent,
}) => {
  const Link = LinkComponent ?? 'a';

  return (
    <Stack direction="row" justify="between" align="center">
      <p className="text-sm text-gray-500">
        {total} result{total !== 1 ? 's' : ''} &mdash; page {page} of{' '}
        {totalPages}
      </p>
      <Stack direction="row" gap="sm">
        {prevHref ? (
          <Link href={prevHref} className={defaultLinkClass}>
            Previous
          </Link>
        ) : (
          <Button variant="outlined" color="tertiary" disabled>
            Previous
          </Button>
        )}
        {nextHref ? (
          <Link href={nextHref} className={defaultLinkClass}>
            Next
          </Link>
        ) : (
          <Button variant="outlined" color="tertiary" disabled>
            Next
          </Button>
        )}
      </Stack>
    </Stack>
  );
};
