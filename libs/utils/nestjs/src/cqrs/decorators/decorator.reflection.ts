import { InstanceWrapper } from '@nestjs/core/injector/instance-wrapper';
import { ICommand } from '@nestjs/cqrs';

import { _ICommandMiddlewareHandler } from '../middlewares';

import { COMMAND_MIDDLEWARE } from './command-middleware.decorator';
import { CommandMiddlewareMetadata } from './command-middleware.decorator.metadata';

export const _getMiddlewareMetadata = <TCommand extends ICommand>(
  // eslint-disable-next-line @typescript-eslint/no-unsafe-function-type
  ctor: Function,
): CommandMiddlewareMetadata<TCommand> | undefined => {
  return Reflect.getMetadata(COMMAND_MIDDLEWARE, ctor);
};

/**
 * Gets the metadata of the processor.
 *
 * @param middleware The middleware to get the metadata from
 * @returns The metadata of the command middleware
 */
export const getMiddlewareMetadata = (
  middleware: InstanceWrapper<_ICommandMiddlewareHandler>,
): CommandMiddlewareMetadata<ICommand> => {
  const metadata = _getMiddlewareMetadata<ICommand>(
    middleware.instance.constructor,
  );

  if (!metadata) {
    throw new Error(`No metadata found for processor ${middleware.name}`);
  }

  return metadata;
};

export const middlewareMetadataExists = (ctor: Function): boolean => {
  return Boolean(_getMiddlewareMetadata(ctor));
};
