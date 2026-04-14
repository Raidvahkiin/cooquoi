import { CommandBus, CqrsModule, QueryBus } from '@nestjs/cqrs';
import { Test } from '@nestjs/testing';
import { DATABASE_TOKEN } from '../../config';
import { MarketModule } from '../../market.module';
import { createPgliteDb } from './pglite';

export async function getTestSuit() {
  const db = await createPgliteDb();

  const module = await Test.createTestingModule({
    imports: [
      CqrsModule,
      MarketModule.register({
        database: { url: 'postgresql://localhost/test' },
      }),
    ],
  })
    .overrideProvider(DATABASE_TOKEN)
    .useValue(db)
    .compile();

  const app = module.createNestApplication();
  await app.init();

  const queryBus = module.get(QueryBus);
  const commandBus = module.get(CommandBus);

  async function clearDb() {
    await db.execute('TRUNCATE TABLE ingredient RESTART IDENTITY CASCADE');
    await db.execute('TRUNCATE TABLE product RESTART IDENTITY CASCADE');
    await db.execute('TRUNCATE TABLE offer RESTART IDENTITY CASCADE');
  }

  return { app, db, module, commandBus, queryBus, clearDb };
}
