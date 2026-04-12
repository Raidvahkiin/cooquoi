import { date, pgTable, text, uuid, varchar } from 'drizzle-orm/pg-core';

export const ingredient = pgTable('ingredient', {
  id: uuid('id').primaryKey().defaultRandom(),
  name: varchar('name', { length: 255 }).notNull().unique(),
  description: text('description'),
  createdAt: date('created_at').notNull().defaultNow(),
  updatedAt: date('updated_at')
    .notNull()
    .defaultNow()
    .$onUpdateFn(() => new Date().toISOString()),
});

export type Ingredient = typeof ingredient.$inferSelect;

export class IngredientEntity {
  constructor(private readonly _state: Ingredient) {}

  get id(): string {
    return this._state.id;
  }

  get name(): string {
    return this._state.name;
  }

  get description(): string | null {
    return this._state.description;
  }

  get createdAt(): Date {
    return new Date(this._state.createdAt);
  }

  get updatedAt(): Date {
    return new Date(this._state.updatedAt);
  }
}
