import { Command } from '@nestjs/cqrs';

import {
  _ICommandMiddlewareHandler,
  CommandMiddlewareContext,
  CommandMiddlewareNext,
  ICommandMiddlewareHandler,
} from '../middlewares';

/**
 * Error thrown when a middleware handler is not found in the container
 */
export class CommandMiddlewareHandlerNotFoundError extends Error {
  constructor(middleware: _ICommandMiddlewareHandler) {
    super(`Middleware ${middleware} is not registered in the container`);
    this.name = 'CommandMiddlewareHandlerNotFoundError';
  }
}

/**
 * The `build` function takes an array of middleware handlers and a final handler and returns a function that processes the middleware chain.
 *
 * @param arr The array of middleware handlers to be processed
 * @param final The final handler to be executed after all middleware handlers
 *
 * @returns A function that takes a command context and returns the result of processing the middleware chain
 */
export const buildChain = <
  TCommand extends Command<TCommandResult>,
  TCommandResult,
>(
  arr: ICommandMiddlewareHandler<TCommand, TCommandResult>[],
  final: CommandMiddlewareNext<TCommand, TCommandResult>,
) => {
  return arr.reduceRight((next, middleware) => {
    if (!middleware) {
      throw new CommandMiddlewareHandlerNotFoundError(middleware);
    }

    return async (context: CommandMiddlewareContext<TCommand>) => {
      return middleware.process(context, next);
    };
  }, final);
};
