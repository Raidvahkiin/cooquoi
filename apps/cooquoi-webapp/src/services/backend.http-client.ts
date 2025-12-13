import { appConfig } from "@/config";
import axios, { AxiosInstance } from "axios";

export interface IngredientDto {
	id: string;
	name: string;
	description: string;
}

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
}

const instance = new BackendClient();

export const backendClient = instance;
