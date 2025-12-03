import { Mocked } from "vitest";

/**
 * Unsafely casts a partial mock object to a fully mocked type.
 *
 * This utility is intended for use in tests where you want to provide only a subset
 * of mocked properties or methods, but need to satisfy the type checker for `Mocked<T>`.
 *
 * @param partial - An object containing a partial implementation of the mocked type.
 * @returns The same object, cast as a fully mocked type.
 *
 * @remarks
 * This function performs an unsafe type cast. It is the caller's responsibility to ensure
 * that the returned object satisfies the contract of `Mocked<T>`. Using this incorrectly
 * may lead to runtime errors if required properties or methods are missing.
 */
export function mockPartial<T>(partial: Partial<Mocked<T>>): Mocked<T> {
	return partial as Mocked<T>;
}
