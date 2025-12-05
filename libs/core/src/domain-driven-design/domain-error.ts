import { ErrorBase } from "../error";

export class DomainError extends ErrorBase {
	constructor(
		public readonly domain: string,
		code: string,
		message: string,
		options?: ErrorOptions,
	) {
		super(code, message, options);
	}
}
