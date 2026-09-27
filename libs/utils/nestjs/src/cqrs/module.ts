import { DynamicModule, Type } from '@nestjs/common';
import { CommandBus, CqrsModule } from '@nestjs/cqrs';

import { ICommandPipeline, CommandPipeline } from './pipeline';

import { Mediator } from './mediator';
import {
  ConfigurableModuleClass,
  MODULE_OPTIONS_TOKEN,
  OPTIONS_TYPE,
} from './config';
import { ILogger, MIDDLEWARE_LOGGER } from './logging/logger';

/**
 * {@link MediatorModule} class
 *
 * This class extends the {@link CqrsModule} and provides a custom implementation of the {@link CommandBus}.
 * It also provides a custom implementation of the {@link CommandPipeline} interface for command processing pipelines.
 */
export class MediatorModule extends ConfigurableModuleClass {
  static override forRoot(options: typeof OPTIONS_TYPE): DynamicModule {
    const mediatorModule = ConfigurableModuleClass.forRoot(options);

    if (!mediatorModule.imports) {
      mediatorModule.imports = [];
    }

    if (!mediatorModule.providers) {
      mediatorModule.providers = [];
    }

    const cqrsModule = CqrsModule.forRoot(options?.cqrs);

    cqrsModule.providers?.push(
      {
        provide: ICommandPipeline,
        useClass: CommandPipeline,
      },
      {
        provide: CommandBus,
        useClass: Mediator,
      },
      {
        provide: MODULE_OPTIONS_TOKEN,
        useValue: options,
      },
    );

    mediatorModule.imports.push(cqrsModule);

    mediatorModule.providers.push(...options.middlewares);

    if (options.loggers && options.loggers.length > 0) {
      mediatorModule.providers.push(
        ...[
          {
            provide: MIDDLEWARE_LOGGER,
            useFactory: (...loggers: ILogger[]) => loggers,
            inject: [...options.loggers] as Type<ILogger>[],
          },
          ...options.loggers,
        ],
      );
    }

    return mediatorModule;
  }

  /**
   * Returns the token used for CommandBus injection
   * This makes the module more testable
   */
  static getCommandBusToken(): Type<CommandBus> {
    return CommandBus;
  }
}
