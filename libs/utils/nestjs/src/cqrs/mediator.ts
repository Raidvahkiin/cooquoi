import { Inject, Injectable, Optional } from '@nestjs/common';
import { ModuleRef } from '@nestjs/core';
import {
  AsyncContext,
  Command,
  CommandBus,
  type CqrsModuleOptions,
} from '@nestjs/cqrs';

// Not in the public exports map of @nestjs/cqrs — use the string token directly
const CQRS_MODULE_OPTIONS = 'CQRS_MODULE_OPTIONS';

import { ICommandPipeline } from './pipeline';

/**
 * {@link Mediator} class
 *
 * This class extends the {@link CommandBus} and provides a custom implementation of the command bus.
 */
@Injectable()
export class Mediator extends CommandBus {
  constructor(
    @Inject(ICommandPipeline)
    private readonly pipeline: ICommandPipeline,

    moduleRef: ModuleRef,

    @Optional()
    @Inject(CQRS_MODULE_OPTIONS)
    options?: CqrsModuleOptions,
  ) {
    super(moduleRef, options);
  }

  override execute<TCommand extends Command<TCommandResult>, TCommandResult>(
    command: TCommand,
    context?: AsyncContext,
  ): Promise<TCommandResult> {
    return this.pipeline.pipe(command, () => super.execute(command, context));
  }
}
