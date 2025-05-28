import { ApiConnector } from "./api-connector";
import Ajv, { JTDSchemaType } from "ajv/dist/jtd";

const ajv = new Ajv();

interface Ingredient {
	id: string;
	name: string;
}

const IngredientSchema: JTDSchemaType<Ingredient> = {
	properties: {
		id: { type: "string" },
		name: { type: "string" },
	},
};

const validate = ajv.compile<Ingredient[]>({
	elements: IngredientSchema,
});

export class IngredientsConnector extends ApiConnector {
	constructor() {
		super("http://localhost:5244");
	}

	getIngredients(): Promise<Ingredient[]> {
		return this.getAndValidateAsync<Ingredient[]>("/ingredients", validate);
	}
}
