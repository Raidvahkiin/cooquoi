'use client';

import {
  type ReactNode,
  type MouseEvent,
  useCallback,
  useEffect,
  useState,
  useRef,
} from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '../../utils/class-name.util';

const idClassname = 'UiModal';

const overlayVariants = cva(
  'fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 transition-opacity duration-200',
  {
    variants: {
      open: {
        true: 'opacity-100 pointer-events-auto',
        false: 'opacity-0 pointer-events-none',
      },
    },
    defaultVariants: { open: false },
  },
);

const panelVariants = cva(
  `
		relative w-full rounded-lg bg-white shadow-xl
		transition-all duration-200
	`,
  {
    variants: {
      size: {
        sm: 'max-w-sm',
        md: 'max-w-md',
        lg: 'max-w-lg',
        xl: 'max-w-xl',
        full: 'max-w-full',
      },
      open: {
        true: 'scale-100 opacity-100',
        false: 'scale-95 opacity-0',
      },
    },
    defaultVariants: { size: 'md', open: false },
  },
);

type ModalSize = NonNullable<VariantProps<typeof panelVariants>['size']>;

export interface ModalProps {
  open: boolean;
  onClose: () => void;
  size?: ModalSize;
  title?: string;
  children?: ReactNode;
  footer?: ReactNode;
  closeOnOverlayClick?: boolean;
  showCloseButton?: boolean;
  className?: string;
}

export const Modal = ({
  open,
  onClose,
  size = 'md',
  title,
  children,
  footer,
  closeOnOverlayClick = true,
  showCloseButton = true,
  className,
}: ModalProps) => {
  const panelRef = useRef<HTMLDivElement>(null);

  const handleOverlayClick = useCallback(
    (e: MouseEvent) => {
      if (closeOnOverlayClick && e.target === e.currentTarget) {
        onClose();
      }
    },
    [closeOnOverlayClick, onClose],
  );

  useEffect(() => {
    if (!open) return;
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [open, onClose]);

  useEffect(() => {
    if (open) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
    return () => {
      document.body.style.overflow = '';
    };
  }, [open]);

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby={title ? 'modal-title' : undefined}
      className={cn(idClassname, overlayVariants({ open }))}
      onClick={handleOverlayClick}
      onKeyDown={(e) => {
        if (e.key === 'Escape') onClose();
      }}
    >
      <div
        ref={panelRef}
        className={cn(panelVariants({ size, open }), className)}
      >
        {(title || showCloseButton) && (
          <div className="flex items-center justify-between border-b border-neutral-200 px-4 py-3 md:px-6 md:py-4">
            {title && (
              <h2
                id="modal-title"
                className="text-lg font-semibold text-neutral-900"
              >
                {title}
              </h2>
            )}
            {showCloseButton && (
              <button
                type="button"
                onClick={onClose}
                className="ml-auto rounded-md p-1 text-neutral-400 transition-colors hover:text-neutral-600"
                aria-label="Close modal"
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  className="size-5"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                  aria-hidden="true"
                >
                  <path
                    fillRule="evenodd"
                    d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                    clipRule="evenodd"
                  />
                </svg>
              </button>
            )}
          </div>
        )}

        <div className="px-4 py-4 md:px-6 md:py-5">{children}</div>

        {footer && (
          <div className="flex items-center justify-end gap-2 border-t border-neutral-200 px-4 py-3 md:px-6 md:py-4">
            {footer}
          </div>
        )}
      </div>
    </div>
  );
};

export const useModal = (initialOpen = false) => {
  const [open, setOpen] = useState(initialOpen);
  const openModal = useCallback(() => setOpen(true), []);
  const closeModal = useCallback(() => setOpen(false), []);
  const toggleModal = useCallback(() => setOpen((prev) => !prev), []);

  return { open, openModal, closeModal, toggleModal };
};
