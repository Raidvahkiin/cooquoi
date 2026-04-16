import { join } from 'node:path';
import { drizzle } from 'drizzle-orm/pglite';
import { PgliteDatabase } from 'drizzle-orm/pglite';
import { migrate } from 'drizzle-orm/pglite/migrator';
import { relations } from '../../domain/entities/relations';

export type PgliteDb = PgliteDatabase<Record<string, never>, typeof relations>;

export async function createPgliteDb(): Promise<PgliteDb> {
  const db = drizzle({ connection: 'memory://', relations });

  await migrate(db, {
    migrationsFolder: join(import.meta.dirname, './drizzle'),
  });

  return db as PgliteDb;
}
