import { date, pgTable, uuid, varchar } from 'drizzle-orm/pg-core';
import { priceColumn } from '../value-objects/price';

export const offers = pgTable('offers', {
  id: uuid('id').primaryKey().defaultRandom(),
  product: uuid('product').notNull(),
  vendor: varchar('vendor', { length: 255 }).notNull(),
  price: priceColumn('price').notNull(),
  ingredients: uuid('ingredients').array(),
  createdAt: date('created_at').notNull().defaultNow(),
  updatedAt: date('updated_at')
    .$onUpdateFn(() => new Date().toISOString())
    .notNull()
    .defaultNow(),
});

export type Offer = typeof offers.$inferSelect;
