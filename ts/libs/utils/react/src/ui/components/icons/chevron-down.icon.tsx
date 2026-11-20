import { ComponentPropsWithoutRef } from 'react';
import { cn } from '../../utils/class-name.util';

export type ChevronDownIconProps = ComponentPropsWithoutRef<'svg'>;

export const ChevronDownIcon = ({
  className,
  fill,
  stroke,
  strokeWidth,
  ...rest
}: ChevronDownIconProps) => {
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
        d="m19.5 8.25-7.5 7.5-7.5-7.5"
      />
    </svg>
  );
};
