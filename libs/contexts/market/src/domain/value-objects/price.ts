import { Price } from '@libs/core';
import { customType } from 'drizzle-orm/pg-core';

export const priceColumn = customType<{ data: Price; driverData: string }>({
  dataType: () => 'varchar',
  toDriver: (value: Price) => value.toString(),
  fromDriver: (value: string) => Price.fromString(value),
});
