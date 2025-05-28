import { ValidateFunction } from "ajv";
import axios, { type AxiosInstance } from "axios";

export abstract class ApiConnector {
	protected client: AxiosInstance;

	constructor(baseUrl: string) {
		this.client = axios.create({
			baseURL: baseUrl,
			headers: {
				"Content-Type": "application/json",
			},
		});
	}

	async getAndValidateAsync<T>(path: string, validate: ValidateFunction<T>) {
		const { data } = await this.client.get<T>(path);
		const isValid = validate(data);
		if (!isValid) {
			throw new Error(
				`Invalid data format received from the API, ${validate.errors?.map((e) => `(${e.keyword} ${e.instancePath}: ${e.message})`).join(",")}`,
			);
		}
		return data;
	}
}
