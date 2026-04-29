'use client';

import { Button, Stack } from '@utils/react/ui';
import Link from 'next/link';
import { usePathname, useSearchParams } from 'next/navigation';

type Props = {
  page: number;
  totalPages: number;
  total: number;
};

const linkClass =
  'inline-flex items-center justify-center rounded px-3 py-1.5 text-sm border border-neutral-500 text-tertiary hover:bg-neutral-100 transition-colors';

export function Pagination({ page, totalPages, total }: Props) {
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const buildHref = (newPage: number) => {
    const params = new URLSearchParams(searchParams.toString());
    params.set('page', String(newPage));
    return `${pathname}?${params.toString()}`;
  };

  return (
    <Stack direction="row" justify="between" align="center">
      <p className="text-sm text-gray-500">
        {total} result{total !== 1 ? 's' : ''} &mdash; page {page} of{' '}
        {totalPages}
      </p>
      <Stack direction="row" gap="sm">
        {page > 1 ? (
          <Link href={buildHref(page - 1)} className={linkClass}>
            Previous
          </Link>
        ) : (
          <Button variant="outlined" color="tertiary" disabled>
            Previous
          </Button>
        )}
        {page < totalPages ? (
          <Link href={buildHref(page + 1)} className={linkClass}>
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
}
