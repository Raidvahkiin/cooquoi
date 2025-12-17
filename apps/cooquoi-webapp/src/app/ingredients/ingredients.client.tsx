"use client";

import type { CreateIngredientDto, IngredientDto } from "@/types/ingredient";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Stack } from "@/components/ui/stack";

import type {
	ColDef,
	GridApi,
	GridReadyEvent,
	IGetRowsParams,
	IDatasource,
} from "ag-grid-community";
import {
	AllCommunityModule,
	ModuleRegistry,
	themeQuartz,
} from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useCallback, useMemo, useRef, useState, useTransition } from "react";

ModuleRegistry.registerModules([AllCommunityModule]);

export const IngredientsClient = () => {
	const [name, setName] = useState("");
	const [description, setDescription] = useState("");
	const [error, setError] = useState<string | null>(null);
	const [isPending, startTransition] = useTransition();
	const gridApiRef = useRef<GridApi | null>(null);

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
				const res = await fetch("/api/ingredients", {
					method: "POST",
					headers: { "Content-Type": "application/json" },
					body: JSON.stringify(dto),
				});
				if (!res.ok) throw new Error("Failed to create ingredient");
				setName("");
				setDescription("");
				refreshGrid();
			} catch (err) {
				setError(err instanceof Error ? err.message : "Unexpected error");
			}
		});
	};

	const onDelete = useCallback(
		(id: string) => {
			setError(null);
			startTransition(async () => {
				try {
					const res = await fetch(`/api/ingredients/${id}`, {
						method: "DELETE",
					});
					if (!res.ok) throw new Error("Failed to delete ingredient");
					refreshGrid();
				} catch (err) {
					setError(err instanceof Error ? err.message : "Unexpected error");
				}
			});
		},
		[refreshGrid],
	);

	const columnDefs = useMemo<ColDef<IngredientDto>[]>(
		() => [
			{
				headerName: "Name",
				field: "name",
				sortable: true,
				filter: "agTextColumnFilter",
				filterParams: {
					filterOptions: ["contains"],
					maxNumConditions: 1,
				},
				flex: 1,
			},
			{
				headerName: "Description",
				field: "description",
				sortable: true,
				filter: false,
				flex: 2,
			},
			{
				headerName: "Actions",
				colId: "actions",
				sortable: true,
				filter: false,
				width: 140,
				valueGetter: () => "",
				cellRenderer: (params: { data?: IngredientDto }) => {
					if (!params.data) return null;
					const id = params.data.id;
					return (
						<Button
							variant="danger"
							disabled={isPending}
							onClick={() => onDelete(id)}
						>
							Remove
						</Button>
					);
				},
			},
		],
		[isPending, onDelete],
	);

	const datasource = useMemo<IDatasource>(
		() => ({
			getRows: async (params: IGetRowsParams) => {
				try {
					const res = await fetch("/api/ingredients/grid", {
						method: "POST",
						headers: { "Content-Type": "application/json" },
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
			</Stack>

			<Card>
				<h2 className="text-lg font-medium text-gray-900">Add ingredient</h2>
				<form className="mt-4" onSubmit={onCreate}>
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

						<div className="pt-2">
							<Button disabled={!canSubmit || isPending} type="submit">
								{isPending ? "Saving..." : "Add"}
							</Button>
						</div>
					</Stack>
				</form>
			</Card>

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
						overlayNoRowsTemplate={"<span>No ingredients yet.</span>"}
					/>
				</div>
			</Stack>
		</Stack>
	);
};
