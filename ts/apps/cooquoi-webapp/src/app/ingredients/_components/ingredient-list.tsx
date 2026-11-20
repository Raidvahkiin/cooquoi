'use client';

import { deleteIngredient } from '@/actions/market.actions';
import type { IngredientDto } from '@/types/ingredient';
import { Button, Stack, TrashIcon } from '@utils/react/ui';
import { useRouter } from 'next/navigation';
import { useTransition } from 'react';

type Props = {
  ingredients: IngredientDto[];
};

export const IngredientList = ({ ingredients }: Props) => {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();

  const onDelete = (id: string) => {
    startTransition(async () => {
      try {
        await deleteIngredient(id);
        router.refresh();
      } catch (err) {
        console.error('Failed to delete ingredient:', err);
      }
    });
  };

  return (
    <>
      <Stack gap="sm">
        {ingredients.length === 0 ? (
          <p className="text-sm text-gray-500">No ingredients yet.</p>
        ) : (
          <ul className="divide-y divide-gray-200 rounded-lg border border-gray-200">
            {ingredients.map((ingredient) => (
              <li
                key={ingredient.id}
                className="flex items-center justify-between px-4 py-3"
              >
                <div>
                  <p className="text-sm font-medium text-gray-900">
                    {ingredient.name}
                  </p>
                  {ingredient.description ? (
                    <p className="text-sm text-gray-500">
                      {ingredient.description}
                    </p>
                  ) : null}
                </div>
                <Button
                  variant="text"
                  color="primary"
                  onClick={() => onDelete(ingredient.id)}
                  disabled={isPending}
                >
                  <TrashIcon />
                </Button>
              </li>
            ))}
          </ul>
        )}
      </Stack>
    </>
  );
};
