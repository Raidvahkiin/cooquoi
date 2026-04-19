import { ComponentPropsWithoutRef } from 'react';
import { cn } from '../../utlis/class-name.util';

export type SearchIconProps = ComponentPropsWithoutRef<'svg'>;

export const SearchIcon = ({
  className,
  fill,
  stroke,
  strokeWidth,
  ...rest
}: SearchIconProps) => {
  return (
    <svg
      {...rest}
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 24 24"
      fill={fill ?? 'none'}
      stroke={stroke ?? 'currentColor'}
      strokeWidth={strokeWidth ?? 1.5}
      className={cn('size-6', className)}
      aria-hidden={true}
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z"
      />
    </svg>
  );
};
