import { Input, PlusIcon, TrashIcon } from '@utils/react/ui';
import type { FieldErrors, UseFormRegister } from 'react-hook-form';

const CURRENCIES = ['EUR', 'USD', 'GBP'] as const;

type OfferValue = {
  vendor: string;
  amount: number;
  currency: (typeof CURRENCIES)[number];
};

export type { OfferValue };
export { CURRENCIES };

export function OfferFields({
  fields,
  errors,
  register,
  onAppend,
  onRemove,
}: {
  fields: { id: string }[];
  errors?: FieldErrors<{ offers: OfferValue[] }>['offers'];
  register: UseFormRegister<{ offers: OfferValue[] }>;
  onAppend: () => void;
  onRemove: (index: number) => void;
}) {
  return (
    <div className="flex flex-col gap-2">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium text-neutral-700">Offers</span>
        <button
          type="button"
          onClick={onAppend}
          className="flex items-center gap-1 text-xs text-primary hover:text-primary-hover font-medium"
        >
          <PlusIcon className="size-3.5" />
          Add offer
        </button>
      </div>

      {fields.length === 0 && (
        <p className="text-xs text-neutral-400">No offers yet — optional.</p>
      )}

      {fields.map((field, index) => (
        <div
          key={field.id}
          className={`flex flex-col gap-1.5 rounded-lg border p-2 ${
            errors?.[index] ? 'border-red-400' : 'border-neutral-200'
          }`}
        >
          <div className="flex items-center gap-2">
            <Input
              placeholder="Vendor name"
              className="flex-1 min-w-0"
              {...register(`offers.${index}.vendor`)}
            />
            <button
              type="button"
              onClick={() => onRemove(index)}
              className="shrink-0 rounded p-1 text-neutral-400 hover:text-red-500 transition-colors"
              aria-label="Remove offer"
            >
              <TrashIcon className="size-4" />
            </button>
          </div>
          {errors?.[index]?.vendor && (
            <p className="text-xs text-red-500">
              {errors[index].vendor?.message}
            </p>
          )}

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
          {errors?.[index]?.amount && (
            <p className="text-xs text-red-500">
              {errors[index].amount?.message}
            </p>
          )}
        </div>
      ))}
    </div>
  );
}
