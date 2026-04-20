import { DynamicModule, Module } from '@nestjs/common';
import { drizzle } from 'drizzle-orm/node-postgres';
import {
  DATABASE_TOKEN,
  type ModuleConfig,
  moduleConfigSchema,
} from './config';
import { schema } from './domain';
import {
  CreateIngredientHandler,
  CreateProductCommandHandler,
  DeleteIngredientCommandHandler,
  FilterIngredientsHandler,
  GetIngredientHandler,
} from './features';

const commandHandlers = [
  CreateIngredientHandler,
  DeleteIngredientCommandHandler,
  CreateProductCommandHandler,
];
const queryHandlers = [GetIngredientHandler, FilterIngredientsHandler];

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
          useValue: drizzle(parsed.database.url, { schema }),
        },
        ...commandHandlers,
        ...queryHandlers,
      ],
    };
  }
}
