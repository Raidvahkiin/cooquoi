import { date, pgTable, text, uuid, varchar } from 'drizzle-orm/pg-core';

export const products = pgTable('products', {
  id: uuid('id').primaryKey().defaultRandom(),
  name: varchar('name', { length: 255 }).notNull().unique(),
  description: text('description'),
  createdAt: date('created_at').notNull().defaultNow(),
  updatedAt: date('updated_at')
    .$onUpdateFn(() => new Date().toISOString())
    .notNull()
    .defaultNow(),
});
