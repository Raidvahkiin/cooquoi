import {
	Enumeration,
	getAllEnumerationInstances,
	getEnumerationInstanceByValue,
} from "./enumeration";

class UpdateStatus extends Enumeration {
	static readonly PENDING = new UpdateStatus("PENDING");
	static readonly IN_PROGRESS = new UpdateStatus("IN_PROGRESS");
	static readonly COMPLETED = new UpdateStatus("COMPLETED");
	static readonly FAILED = new UpdateStatus("FAILED");

	private constructor(value: string) {
		super(value);
	}

	static fromValue(value: string): UpdateStatus {
		return getEnumerationInstanceByValue(UpdateStatus, value);
	}
}

describe("UpdateStatus Enumeration", () => {
	it("should get all instances", () => {
		const instances = getAllEnumerationInstances(UpdateStatus);
		expect(instances).toHaveLength(4);
		expect(instances).toContain(UpdateStatus.PENDING);
		expect(instances).toContain(UpdateStatus.IN_PROGRESS);
		expect(instances).toContain(UpdateStatus.COMPLETED);
		expect(instances).toContain(UpdateStatus.FAILED);
	});

	it("should create instance from value", () => {
		const status = UpdateStatus.fromValue("IN_PROGRESS");
		expect(status).toBe(UpdateStatus.IN_PROGRESS);
	});
	it("should throw error for invalid value", () => {
		expect(() => UpdateStatus.fromValue("INVALID")).toThrow(
			"Enumeration UpdateStatus with value INVALID does not exist",
		);
	});
});
