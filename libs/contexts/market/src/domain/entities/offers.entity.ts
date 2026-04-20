import { date, pgTable, unique, uuid, varchar } from 'drizzle-orm/pg-core';
import { priceColumn } from '../value-objects/price';

export const offers = pgTable(
  'offers',
  {
    id: uuid('id').primaryKey().defaultRandom(),
    productId: uuid('product_id').notNull(),
    vendor: varchar('vendor', { length: 255 }).notNull(),
    price: priceColumn('price').notNull(),
    createdAt: date('created_at').notNull().defaultNow(),
    updatedAt: date('updated_at')
      .$onUpdateFn(() => new Date().toISOString())
      .notNull()
      .defaultNow(),
  },
  (t) => [unique('offers_product_vendor_unique').on(t.productId, t.vendor)],
);

export type Offer = typeof offers.$inferSelect;
