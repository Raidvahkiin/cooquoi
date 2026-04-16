export type Ingredient = {
  id: string;
  name: string;
  description: string | null;
  createdAt: string;
  updatedAt: string;
};

export type CreateIngredientBody = {
  name: string;
  description?: string;
};

export type CreateIngredientResponse = {
  id: string;
};
