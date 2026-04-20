import {
  date,
  numeric,
  pgTable,
  unique,
  uuid,
  varchar,
} from 'drizzle-orm/pg-core';

export const offers = pgTable(
  'offers',
  {
    id: uuid('id').primaryKey().defaultRandom(),
    productId: uuid('product_id').notNull(),
    vendor: varchar('vendor', { length: 255 }).notNull(),
    priceAmount: numeric('price_amount', { precision: 12, scale: 2 }).notNull(),
    priceCurrency: varchar('price_currency', { length: 3 }).notNull(),
    createdAt: date('created_at').notNull().defaultNow(),
    updatedAt: date('updated_at')
      .$onUpdateFn(() => new Date().toISOString())
      .notNull()
      .defaultNow(),
  },
  (t) => [unique('offers_product_vendor_unique').on(t.productId, t.vendor)],
);

export type Offer = typeof offers.$inferSelect;
