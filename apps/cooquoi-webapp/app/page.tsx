import { IngredientsConnector } from "../api-connectors/ingredients-connector";

const ingredientsConnector = new IngredientsConnector();

export default async function Page() {
	const ingredients = await ingredientsConnector.getIngredients();
	return (
		<>
			<h1>Hello, Next.js!</h1>
			{ingredients.map(({ id, name }) => (
				<div key={id}>{name}</div>
			))}
		</>
	);
}
