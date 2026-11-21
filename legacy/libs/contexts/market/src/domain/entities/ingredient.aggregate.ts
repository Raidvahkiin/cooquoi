import {
  type AnyPgColumn,
  date,
  pgTable,
  primaryKey,
  text,
  uuid,
  varchar,
} from 'drizzle-orm/pg-core';

export const ingredients = pgTable('ingredients', {
  id: uuid('id').primaryKey().defaultRandom(),
  name: varchar('name', { length: 255 }).notNull().unique(),
  description: text('description'),
  createdAt: date('created_at').notNull().defaultNow(),
  updatedAt: date('updated_at')
    .notNull()
    .defaultNow()
    .$onUpdateFn(() => new Date().toISOString()),
});

export const ingredientComponents = pgTable(
  'ingredient_components',
  {
    parentIngredientId: uuid('parent_ingredient_id')
      .notNull()
      .references((): AnyPgColumn => ingredients.id, { onDelete: 'cascade' }),
    componentIngredientId: uuid('component_ingredient_id')
      .notNull()
      .references((): AnyPgColumn => ingredients.id, { onDelete: 'cascade' }),
  },
  (t) => [
    primaryKey({
      columns: [t.parentIngredientId, t.componentIngredientId],
    }),
  ],
);

export type Ingredient = typeof ingredients.$inferSelect;
export type IngredientComponent = typeof ingredientComponents.$inferSelect;
