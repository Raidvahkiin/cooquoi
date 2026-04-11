export abstract class Enumeration {
	public readonly value: string;

	protected constructor(value: string) {
		this.value = value;
	}

	equals(other: Enumeration): boolean {
		return (
			other instanceof this.constructor &&
			this.constructor === other.constructor &&
			this.value === other.value
		);
	}
}

export function getAllEnumerationInstances<T extends Enumeration>(enumClass: {
	prototype: T;
}): T[] {
	return Object.values(enumClass).filter(
		(value) =>
			value instanceof (enumClass as abstract new (...args: never[]) => T),
	) as T[];
}

export function getEnumerationInstanceByValue<T extends Enumeration>(
	enumClass: { prototype: T },
	value: string,
): T {
	const instance = getAllEnumerationInstances(enumClass).find(
		(item) => item.value === value,
	);
	if (instance) {
		return instance;
	}
	throw new Error(
		`Enumeration ${enumClass.prototype.constructor.name} with value ${value} does not exist`,
	);
}
