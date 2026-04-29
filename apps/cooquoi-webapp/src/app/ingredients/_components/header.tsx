'use client';

import { createIngredient } from '@/actions/market.actions';
import type { CreateIngredientDto } from '@/types/ingredient';
import {
  Button,
  Input,
  Modal,
  PlusIcon,
  Stack,
  useModal,
} from '@utils/react/ui';
import { useRouter } from 'next/navigation';
import { useState, useTransition } from 'react';

export function Header() {
  const router = useRouter();
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const { open: modalOpen, openModal, closeModal } = useModal();

  const canSubmit = name.trim().length > 0;

  const onCreate = (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    const dto: CreateIngredientDto = {
      name: name.trim(),
      description: description,
    };

    startTransition(async () => {
      try {
        await createIngredient(dto);
        setName('');
        setDescription('');
        closeModal();
        router.refresh();
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Unexpected error');
      }
    });
  };

  return (
    <>
      <Stack direction="row" justify="between" align="start" gap="lg">
        <Stack gap="xs">
          <h1 className="text-2xl font-semibold text-gray-900">Ingredients</h1>
          <p className="text-sm text-gray-600">Manage your ingredients list.</p>
        </Stack>
        <Button variant="contained" color="primary" onClick={openModal}>
          <PlusIcon />
        </Button>
      </Stack>

      <Modal
        open={modalOpen}
        onClose={closeModal}
        title="Add ingredient"
        size="sm"
      >
        <form onSubmit={onCreate}>
          <Stack gap="sm">
            <Stack gap="xs">
              <label
                className="text-sm font-medium text-gray-700"
                htmlFor="name"
              >
                Name
              </label>
              <Input
                id="name"
                name="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Salt"
                autoComplete="off"
              />
            </Stack>

            <Stack gap="xs">
              <label
                className="text-sm font-medium text-gray-700"
                htmlFor="description"
              >
                Description
              </label>
              <Input
                id="description"
                name="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="A common seasoning"
                autoComplete="off"
              />
            </Stack>

            {error ? <p className="text-sm text-red-600">{error}</p> : null}

            <Stack direction="row" justify="end" gap="sm" className="pt-2">
              <Button variant="outlined" color="tertiary" onClick={closeModal}>
                Cancel
              </Button>
              <Button
                variant="contained"
                color="primary"
                disabled={!canSubmit || isPending}
                type="submit"
              >
                {isPending ? 'Saving...' : 'Add'}
              </Button>
            </Stack>
          </Stack>
        </form>
      </Modal>
    </>
  );
}
