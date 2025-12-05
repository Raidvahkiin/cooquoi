import { DomainError } from "@libs/core";
import { DOMAIN_NAME } from "../constants";

export class CooquoiError extends DomainError {
	constructor(code: string, message: string) {
		super(DOMAIN_NAME, code, message);
	}
}
