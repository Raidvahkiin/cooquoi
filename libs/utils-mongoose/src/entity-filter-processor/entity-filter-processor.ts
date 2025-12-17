import { Entity, EntityFilter } from "@libs/core";
export abstract class EntityFilterProcessor<T extends Entity<T>> {
	abstract process(filters: EntityFilter<T>[]): Promise<void>;
}
