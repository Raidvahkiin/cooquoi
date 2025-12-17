import { appConfig } from "@/config";
import type { CreateIngredientDto, IngredientDto } from "@/types/ingredient";
import axios, { AxiosInstance } from "axios";

type AgGridSortDirection = "asc" | "desc";

export type AgGridSortModelItem = {
	colId: string;
	sort: AgGridSortDirection;
};

export type IngredientsGridRequest = {
	startRow: number;
	endRow: number;
	sortModel?: AgGridSortModelItem[];
	filterModel?: Record<string, unknown>;
};

export type IngredientsGridResponse = {
	rows: IngredientDto[];
	lastRow: number;
};

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

	async getIngredientsGrid(
		dto: IngredientsGridRequest,
	): Promise<IngredientsGridResponse> {
		try {
			const response = await this._httpClient.post<IngredientsGridResponse>(
				"ingredients/grid",
				dto,
			);
			return response.data;
		} catch (error) {
			const err = error instanceof Error ? error : new Error(String(error));
			console.error("Error fetching ingredients grid:", err.message, err.stack);
			return { rows: [], lastRow: 0 };
		}
	}
}

const instance = new BackendClient();

export const backendClient = instance;
