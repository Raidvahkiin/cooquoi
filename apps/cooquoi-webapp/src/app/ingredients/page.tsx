import { backendClient } from "@/services";
import { IngredientsClient } from "./ingredients.client";

export default async function IngredientsPage() {
	const ingredients = await backendClient.getIngredients();
	return <IngredientsClient initialIngredients={ingredients} />;
}
