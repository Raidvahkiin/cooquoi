export type IngredientDto = {
	id: string;
	name: string;
	description: string | null;
	createdAt?: string;
	updatedAt?: string;
};

export type CreateIngredientDto = {
	name: string;
	description: string;
};
