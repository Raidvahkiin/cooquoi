import { appConfig } from "@/config";
import type { CreateIngredientDto, IngredientDto } from "@/types/ingredient";
import axios, { AxiosInstance } from "axios";

class BackendClient {
	private readonly _httpClient: AxiosInstance;

	constructor() {
		this._httpClient = axios.create({
			baseURL: appConfig.backend.baseUrl,
		});
	}

	async getIngredients(): Promise<IngredientDto[]> {
		try {
			const response =
				await this._httpClient.get<IngredientDto[]>("ingredients");
			return response.data;
		} catch (error) {
			const err = error instanceof Error ? error : new Error(String(error));
			console.error("Error fetching ingredients:", err.message, err.stack);
			return [];
		}
	}

	async createIngredient(dto: CreateIngredientDto): Promise<void> {
		try {
			await this._httpClient.post("ingredients", dto);
		} catch (error) {
			const err = error instanceof Error ? error : new Error(String(error));
			console.error("Error creating ingredient:", err.message, err.stack);
			throw err;
		}
	}

	async deleteIngredient(id: string): Promise<void> {
		try {
			await this._httpClient.delete(`ingredients/${id}`);
		} catch (error) {
			const err = error instanceof Error ? error : new Error(String(error));
			console.error("Error deleting ingredient:", err.message, err.stack);
			throw err;
		}
	}
}

const instance = new BackendClient();

export const backendClient = instance;
