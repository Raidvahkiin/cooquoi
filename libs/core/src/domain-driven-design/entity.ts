import { DatetimeProvider, SystemDatetimeProvider } from "../datetime";
import { v4 as uuidv4 } from "uuid";

export interface EntityProps {
	id?: string;
	createdAt?: Date;
	updatedAt?: Date;
}

export interface EntityUpdateOptions {
	datetimeProvider: DatetimeProvider;
}

export abstract class Entity<T extends Entity<T>> {
	private _id: string;
	private _createdAt: Date;
	private _updatedAt: Date;

	constructor(props?: EntityProps, options?: EntityUpdateOptions) {
		const { datetimeProvider = new SystemDatetimeProvider() } = options ?? {};
		const { id, createdAt, updatedAt } = props ?? {};

		const initialTimestamp = datetimeProvider.now();

		this._id = id ?? uuidv4();

		this._createdAt = createdAt ?? initialTimestamp;
		this._updatedAt = updatedAt ?? initialTimestamp;
	}

	public get id() {
		return this._id;
	}

	public get createdAt() {
		return this._createdAt;
	}

	public get updatedAt() {
		return this._updatedAt;
	}

	/**
	 * Generic method to update the state of the entity.
	 *
	 * Ignores by default the keys `id`, `createdAt`, and `updatedAt`.
	 * Can be extended to ignore more keys by providing them in the omitKeys array.
	 *
	 * **Important**: The keys in newState must match the private properties of the class (with a leading underscore).
	 *
	 * Update the `updatedAt` property if any state change occurs.
	 */
	protected commitUpdate({
		newState,
		omitKeys = [],
		datetimeProvider,
	}: {
		newState: Partial<Omit<T, "id" | "createdAt" | "updatedAt">>;
		omitKeys?: (keyof T)[];
		datetimeProvider: DatetimeProvider;
	}): void {
		let stateChanged = false;
		const keys = (Object.keys(newState) as (keyof typeof newState)[]).filter(
			(key) => {
				return ![...omitKeys, "id", "createdAt", "updatedAt"].includes(key);
			},
		);

		for (const key of keys) {
			if (typeof key !== "string") {
				continue;
			}

			const privateKey = `_${key}` as keyof this;
			const value = newState[key];

			if (value === undefined || !(privateKey in this)) {
				continue;
			}

			if (this[privateKey] === value) {
				continue;
			}

			this[privateKey] = value as this[keyof this];
			stateChanged = true;
		}

		if (stateChanged) {
			this._updatedAt = datetimeProvider.now();
		}
	}
}
