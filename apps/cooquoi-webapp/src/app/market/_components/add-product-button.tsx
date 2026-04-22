'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import {
  Button,
  Input,
  Modal,
  PlusIcon,
  TrashIcon,
  useModal,
} from '@utils/react/ui';
import { useCallback, useEffect, useRef, useState, useTransition } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
import { useRouter } from 'next/navigation';
import { z } from 'zod';

const CURRENCIES = ['EUR', 'USD', 'GBP'] as const;

const offerSchema = z.object({
  vendor: z.string().min(1, 'Vendor is required'),
  amount: z
    .string()
    .min(1, 'Amount is required')
    .refine((v) => Number(v) > 0, { message: 'Must be greater than 0' }),
  currency: z.enum(CURRENCIES),
});

const schema = z.object({
  name: z.string().min(1, 'Name is required'),
  description: z.string().optional(),
  offers: z.array(offerSchema).superRefine((offers, ctx) => {
    const vendors = offers.map((o) => o.vendor.trim().toLowerCase());
    offers.forEach((offer, i) => {
      const v = offer.vendor.trim().toLowerCase();
      if (v && vendors.indexOf(v) !== i) {
        ctx.addIssue({
          code: 'custom',
          message: 'Vendor already added.',
          path: [i, 'vendor'],
        });
      }
    });
  }),
});

type FormValues = z.infer<typeof schema>;

export function AddProductButton() {
  const { open, openModal, closeModal } = useModal();
  const router = useRouter();
  const [checkingName, setCheckingName] = useState(false);
  const [nameExists, setNameExists] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const {
    register,
    control,
    handleSubmit,
    watch,
    reset,
    formState: { errors, isValid },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', description: '', offers: [] },
    mode: 'onTouched',
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'offers' });

  const nameValue = watch('name');

  // Debounced async name uniqueness check (kept outside Zod — it's async/side-effectful)
  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    const trimmed = nameValue?.trim();
    if (!trimmed) {
      setNameExists(false);
      setCheckingName(false);
      return;
    }
    setCheckingName(true);
    debounceRef.current = setTimeout(async () => {
      try {
        const res = await fetch(
          `/api/products/check-name?name=${encodeURIComponent(trimmed)}`,
        );
        const data = await res.json();
        setNameExists(data.exists);
      } finally {
        setCheckingName(false);
      }
    }, 400);
  }, [nameValue]);

  const handleClose = useCallback(() => {
    reset();
    setNameExists(false);
    setCheckingName(false);
    setSubmitError(null);
    closeModal();
  }, [closeModal, reset]);

  const onSubmit = (values: FormValues) => {
    setSubmitError(null);
    startTransition(async () => {
      try {
        const productRes = await fetch('/api/products', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            name: values.name.trim(),
            description: values.description?.trim() || undefined,
            ingredients: [],
          }),
        });
        if (!productRes.ok) {
          const data = await productRes.json();
          throw new Error(data.message ?? 'Failed to create product');
        }
        const product = await productRes.json();

        if (values.offers.length > 0) {
          const offerRes = await fetch('/api/offers', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(
              values.offers.map((offer) => ({
                productId: product.id,
                vendor: offer.vendor.trim(),
                price: { amount: offer.amount, currency: offer.currency },
              })),
            ),
          });
          if (!offerRes.ok) {
            const data = await offerRes.json();
            throw new Error(data.message ?? 'Failed to create offers');
          }
        }

        handleClose();
        router.refresh();
      } catch (err) {
        setSubmitError(err instanceof Error ? err.message : 'Unexpected error');
      }
    });
  };

  const canSubmit = isValid && !nameExists && !checkingName && !isPending;

  return (
    <>
      <button
        type="button"
        onClick={openModal}
        className="flex h-9 w-9 items-center justify-center rounded-full bg-primary text-white shadow hover:bg-primary-hover transition-colors"
        aria-label="Add product"
      >
        <PlusIcon className="size-5" />
      </button>

      <Modal
        open={open}
        onClose={handleClose}
        title="Add product"
        size="lg"
        footer={
          <>
            <Button
              variant="outlined"
              color="tertiary"
              onClick={handleClose}
              type="button"
            >
              Cancel
            </Button>
            <Button type="submit" form="add-product-form" disabled={!canSubmit}>
              {isPending ? 'Creating…' : 'Create'}
            </Button>
          </>
        }
      >
        <form
          id="add-product-form"
          onSubmit={handleSubmit(onSubmit)}
          className="flex flex-col gap-4"
        >
          {/* Product name */}
          <div className="flex flex-col gap-1">
            <label
              className="text-sm font-medium text-neutral-700"
              htmlFor="product-name"
            >
              Name <span className="text-red-500">*</span>
            </label>
            <Input
              id="product-name"
              placeholder="e.g. Atlantic Salmon"
              autoFocus
              {...register('name')}
            />
            {checkingName && (
              <p className="text-xs text-neutral-400">Checking name…</p>
            )}
            {!checkingName && errors.name && (
              <p className="text-xs text-red-500">{errors.name.message}</p>
            )}
            {!checkingName && !errors.name && nameExists && (
              <p className="text-xs text-red-500">
                A product with this name already exists.
              </p>
            )}
          </div>

          {/* Description */}
          <div className="flex flex-col gap-1">
            <label
              className="text-sm font-medium text-neutral-700"
              htmlFor="product-desc"
            >
              Description
            </label>
            <textarea
              id="product-desc"
              placeholder="Optional description…"
              rows={2}
              className="w-full rounded-md border border-border bg-surface px-3 py-2 text-sm text-text placeholder:text-text-muted focus:outline-none focus:ring-2 focus:ring-primary-ring resize-none"
              {...register('description')}
            />
          </div>

          {/* Offers */}
          <div className="flex flex-col gap-2">
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-neutral-700">
                Offers
              </span>
              <button
                type="button"
                onClick={() =>
                  append({ vendor: '', amount: '', currency: 'EUR' })
                }
                className="flex items-center gap-1 text-xs text-primary hover:text-primary-hover font-medium"
              >
                <PlusIcon className="size-3.5" />
                Add offer
              </button>
            </div>

            {fields.length === 0 && (
              <p className="text-xs text-neutral-400">
                No offers yet — optional.
              </p>
            )}

            {fields.map((field, index) => (
              <div
                key={field.id}
                className={`flex flex-col gap-1.5 rounded-lg border p-2 ${
                  errors.offers?.[index]
                    ? 'border-red-400'
                    : 'border-neutral-200'
                }`}
              >
                {/* Row 1: vendor name + remove button */}
                <div className="flex items-center gap-2">
                  <Input
                    placeholder="Vendor name"
                    className="flex-1 min-w-0"
                    {...register(`offers.${index}.vendor`)}
                  />
                  <button
                    type="button"
                    onClick={() => remove(index)}
                    className="shrink-0 rounded p-1 text-neutral-400 hover:text-red-500 transition-colors"
                    aria-label="Remove offer"
                  >
                    <TrashIcon className="size-4" />
                  </button>
                </div>
                {errors.offers?.[index]?.vendor && (
                  <p className="text-xs text-red-500">
                    {errors.offers[index].vendor?.message}
                  </p>
                )}
                {/* Row 2: price amount + currency */}
                <div className="flex items-center gap-2">
                  <Input
                    type="number"
                    placeholder="Price"
                    min="0"
                    step="0.01"
                    className="flex-1 min-w-0"
                    {...register(`offers.${index}.amount`)}
                  />
                  <select
                    className="rounded-md border border-border bg-surface px-2 py-2 text-sm text-text focus:outline-none focus:ring-2 focus:ring-primary-ring"
                    {...register(`offers.${index}.currency`)}
                  >
                    {CURRENCIES.map((c) => (
                      <option key={c} value={c}>
                        {c}
                      </option>
                    ))}
                  </select>
                </div>
                {errors.offers?.[index]?.amount && (
                  <p className="text-xs text-red-500">
                    {errors.offers[index].amount?.message}
                  </p>
                )}
              </div>
            ))}
          </div>

          {submitError && (
            <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-600">
              {submitError}
            </p>
          )}
        </form>
      </Modal>
    </>
  );
}
