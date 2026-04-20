'use client';

import { cva, VariantProps } from 'class-variance-authority';
import type { ButtonHTMLAttributes, FC } from 'react';
import { cn } from '../../utils/class-name.util';

const idClassname = 'UiButton';

const buttonVariants = cva(
  `
		rounded-md px-3 py-2
		inline-flex items-center justify-center
		text-sm font-sans font-normal
		cursor-pointer

		disabled:opacity-50 disabled:pointer-events-none

		md:px-4 md:py-2.5 md:text-base
	`,
  {
    variants: {
      variant: {
        contained: 'border border-transparent',
        outlined: `
					border border-neutral-500 bg-transparent
				`,
        text: 'border-0 bg-transparent text-neutral-500',
      },
      color: {
        primary: '',
        secondary: '',
        tertiary: '',
      },
    },
    compoundVariants: [
      {
        variant: 'contained',
        color: 'primary',
        className: `
					bg-primary text-neutral-100
					hover:bg-primary-hover
					active:bg-primary/90
					disabled:opacity-50
				`,
      },
      {
        variant: 'contained',
        color: 'secondary',
        className: `
					bg-secondary text-neutral-100
					hover:bg-secondary-hover
					active:bg-secondary/90
					disabled:opacity-50
				`,
      },
      {
        variant: 'contained',
        color: 'tertiary',
        className: `
					bg-neutral-500 text-white
					hover:bg-neutral-600
					active:bg-neutral-700
					disabled:opacity-50
				`,
      },
      {
        variant: 'outlined',
        color: 'primary',
        className: `
					text-primary border-primary
				`,
      },
      {
        variant: 'outlined',
        color: 'secondary',
        className: `
					text-secondary border-secondary
				`,
      },
      {
        variant: 'outlined',
        color: 'tertiary',
        className: `
					text-tertiary border-tertiary
				`,
      },
      {
        variant: 'text',
        color: 'primary',
        className: `
					text-primary
					hover:text-primary-hover
				`,
      },
      {
        variant: 'text',
        color: 'secondary',
        className: `
					text-secondary
					hover:text-secondary-hover
				`,
      },
      {
        variant: 'text',
        color: 'tertiary',
        className: `
					text-text
					hover:text-text/70
				`,
      },
    ],
    defaultVariants: {
      variant: 'contained',
      color: 'primary',
    },
  },
);

export const Button: FC<
  ButtonHTMLAttributes<HTMLButtonElement> & VariantProps<typeof buttonVariants>
> = ({
  children,
  className,
  variant = 'contained',
  color = 'primary',
  type = 'button',
  ...rest
}) => {
  return (
    <button
      type={type}
      className={cn(
        idClassname,
        variant,
        color,
        buttonVariants({ variant, color }),
        className,
      )}
      {...rest}
    >
      {children}
    </button>
  );
};
