import { Type, InjectableOptions, Injectable } from '@nestjs/common';
import { ICommand } from '@nestjs/cqrs';

import { CommandMiddlewareMetadata } from './command-middleware.decorator.metadata';

export const COMMAND_MIDDLEWARE = Symbol('CommandMiddleware');

/**
 * {@link CommandMiddleware} decorator
 *
 * This decorator is used to mark a class as a command pipeline processor.
 *
 * @param hook The hook type (pre or post) for the command pipeline processor. It determines when the processor will be executed in the command pipeline.
 * @param type The type of command that the processor is associated with. This is used to filter the processors based on the command type when executing the pipeline.
 * @param options Optional InjectableOptions for the processor. This allows you to specify additional options for the processor, such as scope or dependencies if needed in the future.
 */
export function CommandMiddleware<TCommand extends ICommand>(
  type: Type<TCommand>,
  options?: InjectableOptions,
): ClassDecorator {
  return (target: any) => {
    const metadata: CommandMiddlewareMetadata<TCommand> = {
      type,
    };

    // Define metadata for the command pipeline processor.
    Reflect.defineMetadata(COMMAND_MIDDLEWARE, metadata, target);

    return Injectable(options)(target);
  };
}
