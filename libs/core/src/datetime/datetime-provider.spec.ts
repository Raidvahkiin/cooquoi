import { describe, it } from "vitest";
import { SystemDatetimeProvider } from "./datetime-provider";

describe("SystemDatetimeProvider", () => {
	it("should return current date", () => {
		const datetimeProvider = new SystemDatetimeProvider();
		const now = new Date();
		const providedNow = datetimeProvider.now();

		// Allow a small delta for execution time
		const delta = Math.abs(providedNow.getTime() - now.getTime());
		if (delta > 1000) {
			throw new Error(
				`DatetimeProvider returned a time too far from now: ${providedNow} vs ${now}`,
			);
		}
	});
});
