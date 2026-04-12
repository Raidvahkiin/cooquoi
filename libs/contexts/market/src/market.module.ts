import { DynamicModule, Module } from '@nestjs/common';
import { drizzle } from 'drizzle-orm/node-postgres';
import {
  DATABASE_TOKEN,
  type ModuleConfig,
  moduleConfigSchema,
} from './config';
import {
  CreateIngredientHandler,
  GetIngredientHandler,
  IngredientController,
} from './features';

const commandHandlers = [CreateIngredientHandler];
const queryHandlers = [GetIngredientHandler];

@Module({})
export class MarketModule {
  readonly name = 'MarketModule';

  static register(config: ModuleConfig): DynamicModule {
    const parsed = moduleConfigSchema.parse(config);
    return {
      module: MarketModule,
      providers: [
        {
          provide: DATABASE_TOKEN,
          useValue: drizzle(parsed.database.url),
        },
        ...commandHandlers,
        ...queryHandlers,
      ],
      controllers: [IngredientController],
    };
  }
}
