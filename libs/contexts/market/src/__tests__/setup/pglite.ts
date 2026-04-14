import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { PGlite } from '@electric-sql/pglite';
import { drizzle } from 'drizzle-orm/pglite';

const MIGRATIONS_DIR = join(import.meta.dirname, '../setup/drizzle');

function loadMigrationsSql(): string {
  const journal = JSON.parse(
    readFileSync(join(MIGRATIONS_DIR, 'meta/_journal.json'), 'utf-8'),
  ) as { entries: { tag: string }[] };

  return journal.entries
    .map(({ tag }) => readFileSync(join(MIGRATIONS_DIR, `${tag}.sql`), 'utf-8'))
    .join('\n');
}

export async function createPgliteDb() {
  const client = new PGlite();
  await client.exec(loadMigrationsSql());
  return drizzle(client);
}

export type PgliteDb = Awaited<ReturnType<typeof createPgliteDb>>;
