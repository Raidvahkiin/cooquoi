export interface ValueObject {
	toString(): string;
	equals(vo: ValueObject | undefined | null): boolean;
	compareTo(vo: ValueObject | undefined | null): number;
}
