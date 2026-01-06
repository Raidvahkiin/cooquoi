import { ERROR_CODES } from "../constants";
import { CooquoiError } from "./cooquoi.error";

export class ProductCreationError extends CooquoiError {
	constructor(message: string) {
		super(ERROR_CODES.PRODUCT_CREATION, message);
	}
}
