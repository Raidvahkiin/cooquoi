'use client';

import type { CreateIngredientDto, IngredientDto } from '@/types/ingredient';
import { Button, Input, Modal, Stack, useModal } from '@utils/react/ui';
import type {
  ColDef,
  GridApi,
  GridReadyEvent,
  IDatasource,
  IGetRowsParams,
} from 'ag-grid-community';
import {
  AllCommunityModule,
  ModuleRegistry,
  themeQuartz,
} from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import { useCallback, useMemo, useRef, useState, useTransition } from 'react';

ModuleRegistry.registerModules([AllCommunityModule]);

export const IngredientsClient = () => {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();
  const gridApiRef = useRef<GridApi | null>(null);

  const { open: modalOpen, openModal, closeModal } = useModal();

  const canSubmit = useMemo(() => name.trim().length > 0, [name]);

  const refreshGrid = useCallback(() => {
    gridApiRef.current?.purgeInfiniteCache();
  }, []);

  const onCreate = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    const dto: CreateIngredientDto = {
      name: name.trim(),
      description: description,
    };

    startTransition(async () => {
      try {
        const res = await fetch('/api/ingredients', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(dto),
        });
        if (!res.ok) throw new Error('Failed to create ingredient');
        setName('');
        setDescription('');
        closeModal();
        refreshGrid();
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Unexpected error');
      }
    });
  };

  const columnDefs = useMemo<ColDef<IngredientDto>[]>(
    () => [
      {
        headerName: 'Name',
        field: 'name',
        sortable: true,
        filter: 'agTextColumnFilter',
        filterParams: {
          filterOptions: [
            {
              displayKey: 'fuzzy',
              displayName: 'Fuzzy',
              predicate: (filterValue?: string, cellValue?: string) => {
                const needle = (filterValue ?? '').trim().toLowerCase();
                const haystack = (cellValue ?? '').toLowerCase();
                if (needle.length === 0) return true;
                return haystack.includes(needle);
              },
            },
          ],
          defaultOption: 'fuzzy',
          maxNumConditions: 1,
        },
        flex: 1,
      },
      {
        headerName: 'Description',
        field: 'description',
        sortable: true,
        filter: false,
        flex: 2,
      },
    ],
    [],
  );

  const datasource = useMemo<IDatasource>(
    () => ({
      getRows: async (params: IGetRowsParams) => {
        try {
          const res = await fetch('/api/ingredients/grid', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
              startRow: params.startRow,
              endRow: params.endRow,
              sortModel: params.sortModel,
              filterModel: params.filterModel,
            }),
          });
          if (!res.ok) {
            params.failCallback();
            return;
          }

          const data = (await res.json()) as {
            rows: IngredientDto[];
            lastRow: number;
          };

          params.successCallback(data.rows, data.lastRow);
        } catch {
          params.failCallback();
        }
      },
    }),
    [],
  );

  const onGridReady = (event: GridReadyEvent) => {
    gridApiRef.current = event.api;
  };

  return (
    <Stack className="mx-auto w-full max-w-3xl px-4 py-6" gap="lg">
      <Stack direction="row" justify="between" align="start" gap="lg">
        <Stack gap="xs">
          <h1 className="text-2xl font-semibold text-gray-900">Ingredients</h1>
          <p className="text-sm text-gray-600">Manage your ingredients list.</p>
        </Stack>
        <Button variant="contained" color="primary" onClick={openModal}>
          Add Ingredient
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

      <Stack gap="sm">
        <h2 className="text-lg font-medium text-gray-900">All ingredients</h2>
        <div className="w-full" style={{ height: 520 }}>
          <AgGridReact<IngredientDto>
            theme={themeQuartz}
            columnDefs={columnDefs}
            defaultColDef={{ resizable: true, sortable: true, filter: false }}
            rowModelType="infinite"
            datasource={datasource}
            cacheBlockSize={50}
            maxBlocksInCache={5}
            onGridReady={onGridReady}
            overlayNoRowsTemplate={'<span>No ingredients yet.</span>'}
          />
        </div>
      </Stack>
    </Stack>
  );
};
