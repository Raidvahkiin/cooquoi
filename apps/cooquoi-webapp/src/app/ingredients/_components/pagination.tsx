'use client';

import { Pagination as UiPagination } from '@utils/react/ui';
import Link from 'next/link';
import { usePathname, useSearchParams } from 'next/navigation';

type Props = {
  page: number;
  totalPages: number;
  total: number;
};

export function Pagination({ page, totalPages, total }: Props) {
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const buildHref = (newPage: number) => {
    const params = new URLSearchParams(searchParams.toString());
    params.set('page', String(newPage));
    return `${pathname}?${params.toString()}`;
  };

  return (
    <UiPagination
      page={page}
      totalPages={totalPages}
      total={total}
      prevHref={page > 1 ? buildHref(page - 1) : undefined}
      nextHref={page < totalPages ? buildHref(page + 1) : undefined}
      LinkComponent={Link}
    />
  );
}
