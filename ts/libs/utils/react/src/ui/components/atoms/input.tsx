import type { FC, InputHTMLAttributes, ReactNode } from 'react';

export type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  startComponent?: ReactNode;
  endComponent?: ReactNode;
};

export const Input: FC<InputProps> = ({
  className,
  startComponent,
  endComponent,
  ...rest
}) => {
  if (!startComponent && !endComponent) {
    return (
      <input
        className={`w-full rounded-md border border-border bg-surface px-3 py-2 text-sm text-text placeholder:text-text-muted focus:outline-none focus:ring-2 focus:ring-primary-ring ${className ?? ''}`}
        {...rest}
      />
    );
  }

  return (
    <div className="relative flex w-full items-center">
      {startComponent && (
        <span className="pointer-events-none absolute left-3 flex items-center text-text-muted">
          {startComponent}
        </span>
      )}
      <input
        className={`w-full rounded-md border border-border bg-surface py-2 text-sm text-text placeholder:text-text-muted focus:outline-none focus:ring-2 focus:ring-primary-ring ${
          startComponent ? 'pl-9' : 'pl-3'
        } ${endComponent ? 'pr-9' : 'pr-3'} ${className ?? ''}`}
        {...rest}
      />
      {endComponent && (
        <span className="pointer-events-none absolute right-3 flex items-center text-text-muted">
          {endComponent}
        </span>
      )}
    </div>
  );
};
