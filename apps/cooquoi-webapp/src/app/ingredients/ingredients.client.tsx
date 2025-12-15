"use client";

import type { CreateIngredientDto, IngredientDto } from "@/types/ingredient";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Stack } from "@/components/ui/stack";
import { useMemo, useState, useTransition } from "react";

type Props = {
	initialIngredients: IngredientDto[];
};

export const IngredientsClient = ({ initialIngredients }: Props) => {
	const [ingredients, setIngredients] =
		useState<IngredientDto[]>(initialIngredients);
	const [name, setName] = useState("");
	const [description, setDescription] = useState("");
	const [error, setError] = useState<string | null>(null);
	const [isPending, startTransition] = useTransition();

	const canSubmit = useMemo(() => name.trim().length > 0, [name]);

	const refresh = async () => {
		const res = await fetch("/api/ingredients", { cache: "no-store" });
		if (!res.ok) throw new Error("Failed to refresh ingredients");
		const next = (await res.json()) as IngredientDto[];
		setIngredients(next);
	};

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
				await refresh();
			} catch (err) {
				setError(err instanceof Error ? err.message : "Unexpected error");
			}
		});
	};

	const onDelete = (id: string) => {
		setError(null);
		startTransition(async () => {
			try {
				const res = await fetch(`/api/ingredients/${id}`, {
					method: "DELETE",
				});
				if (!res.ok) throw new Error("Failed to delete ingredient");
				setIngredients((prev) => prev.filter((i) => i.id !== id));
			} catch (err) {
				setError(err instanceof Error ? err.message : "Unexpected error");
			}
		});
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
				<Stack gap="sm">
					{ingredients.length === 0 ? (
						<Card>
							<p className="text-sm text-gray-600">No ingredients yet.</p>
						</Card>
					) : (
						ingredients.map((ingredient) => (
							<Card key={ingredient.id} className="p-4">
								<Stack direction="row" justify="between" align="start" gap="lg">
									<Stack gap="xs">
										<p className="font-medium text-gray-900">
											{ingredient.name}
										</p>
										{ingredient.description ? (
											<p className="text-sm text-gray-600">
												{ingredient.description}
											</p>
										) : null}
									</Stack>
									<Button
										variant="danger"
										disabled={isPending}
										onClick={() => onDelete(ingredient.id)}
									>
										Remove
									</Button>
								</Stack>
							</Card>
						))
					)}
				</Stack>
			</Stack>
		</Stack>
	);
};
