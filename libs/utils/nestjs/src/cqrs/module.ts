import { DynamicModule, Type } from '@nestjs/common';
import { CommandBus, CqrsModule } from '@nestjs/cqrs';

import { ICommandPipeline, CommandPipeline } from './pipeline';

import { Mediator } from './mediator';
import {
  ConfigurableModuleClass,
  MODULE_OPTIONS_TOKEN,
  OPTIONS_TYPE,
} from './config';

/**
 * {@link MediatorModule} class
 *
 * This class extends the {@link CqrsModule} and provides a custom implementation of the {@link CommandBus}.
 * It also provides a custom implementation of the {@link CommandPipeline} interface for command processing pipelines.
 */
export class MediatorModule extends ConfigurableModuleClass {
  static override forRoot(options: typeof OPTIONS_TYPE): DynamicModule {
    const mediatorModule = super.forRoot(options);

    if (!mediatorModule.imports) {
      mediatorModule.imports = [];
    }

    if (!mediatorModule.providers) {
      mediatorModule.providers = [];
    }

    // Add the CqrsModule to the imports of the MediatorModule
    {
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
    }

    // Add the MediatorModule to the providers of the MediatorModule
    {
      mediatorModule.providers.push(...options.middlewares);
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
