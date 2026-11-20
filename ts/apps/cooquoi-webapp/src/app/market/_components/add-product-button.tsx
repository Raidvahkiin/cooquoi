'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { Button, Input, Modal, PlusIcon, useModal } from '@utils/react/ui';
import { useCallback, useEffect, useState, useTransition } from 'react';
import { useDebounce } from '@utils/react/hooks';
import {
  checkProductNameExists,
  createOffers,
  createProduct,
} from '@/actions/market.actions';
import { useFieldArray, useForm } from 'react-hook-form';
import { useRouter } from 'next/navigation';
import { z } from 'zod';
import { IngredientPicker } from './ingredient-picker';
import { CURRENCIES, OfferFields } from './offer-fields';

const offerSchema = z.object({
  vendor: z.string().min(1, 'Vendor is required'),
  amount: z.number().nonnegative(),
  currency: z.enum(CURRENCIES),
});

const schema = z.object({
  name: z.string().min(1, 'Name is required'),
  description: z.string().optional(),
  ingredientIds: z.array(z.string()),
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

  const {
    register,
    control,
    handleSubmit,
    watch,
    reset,
    setValue,
    formState: { errors, isValid },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', description: '', ingredientIds: [], offers: [] },
    mode: 'onTouched',
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'offers' });

  const nameValue = watch('name');
  const selectedIngredientIds = watch('ingredientIds');
  const debouncedName = useDebounce(nameValue, 400);

  // Debounced async name uniqueness check (kept outside Zod — it's async/side-effectful)
  useEffect(() => {
    const trimmed = debouncedName?.trim();
    if (!trimmed) {
      setNameExists(false);
      setCheckingName(false);
      return;
    }
    setCheckingName(true);
    checkProductNameExists(trimmed)
      .then((exists) => setNameExists(exists))
      .finally(() => setCheckingName(false));
  }, [debouncedName]);

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
        const product = await createProduct({
          name: values.name.trim(),
          description: values.description?.trim() || undefined,
          ingredients: values.ingredientIds,
        });

        if (values.offers.length > 0) {
          await createOffers(
            values.offers.map((offer) => ({
              productId: product.id,
              vendor: offer.vendor.trim(),
              price: { amount: offer.amount, currency: offer.currency },
            })),
          );
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

          <IngredientPicker
            selectedIds={selectedIngredientIds}
            onChange={(ids) => setValue('ingredientIds', ids)}
          />

          <OfferFields
            fields={fields}
            errors={errors.offers}
            register={register}
            onAppend={() => append({ vendor: '', amount: 0, currency: 'EUR' })}
            onRemove={remove}
          />

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
