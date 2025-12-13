import { backendClient, IngredientDto } from "@/services";

export default async function IngredientsPage() {
	let ingredients: IngredientDto[] = [];
	try {
		ingredients = await backendClient.getIngredients();
	} catch (error) {
		// Optionally, log or display error
		return <div>Failed to load ingredients.</div>;
	}

	return (
		<div>
			<h1>Ingredients</h1>
			{ingredients.map((ingredient) => (
				<div key={ingredient.id}>
					<h2>{ingredient.name}</h2>
					<p>{ingredient.description}</p>
				</div>
			))}
		</div>
	);
}
