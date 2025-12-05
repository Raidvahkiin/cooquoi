import { ERROR_CODES } from "../constants";
import { CooquoiError } from "./cooquoi.error";

export class InvalidOperationError extends CooquoiError {
	constructor(message: string) {
		super(ERROR_CODES.INVALID_OPERATION, message);
	}
}
